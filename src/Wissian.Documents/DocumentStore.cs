using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Wissian.Documents;

public sealed record DocumentCommand(Guid CommandId, long ExpectedSessionVersion, Guid? DocumentId,
    long? ExpectedVersion, string Action, DocumentInput? Value, int? Revision = null);
public sealed record DocumentMutation(Guid Id, long Version, int Revision, string? ScanState, Guid CommandId, bool Replayed);
public sealed record DocumentPage(IReadOnlyList<DocumentView> Items, int Page, int Total);
public sealed record DocumentFile(RevisionView Revision, byte[] Bytes);

public sealed class DocumentStore(DocumentsDbContext db)
{
    private void RequireTransaction()
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("Operação requer transação autorizada.");
    }
    public async Task<DocumentPage> SearchAsync(Guid organization, string? query, string status, int page, CancellationToken ct)
    {
        RequireTransaction(); query = query?.Trim();
        if (query?.Length > 100 || page is < 1 or > 10000 || status is not ("active" or "archived" or "all"))
            throw new ArgumentException("Filtro inválido.");
        var rows = db.Documents.AsNoTracking().Where(x => x.OrganizationId == organization);
        if (status != "all") rows = rows.Where(x => x.IsArchived == (status == "archived"));
        if (!string.IsNullOrEmpty(query))
        {
            var term = query.ToLowerInvariant();
            rows = rows.Where(x => x.Title.ToLower().Contains(term) || x.Source != null && x.Source.ToLower().Contains(term));
        }
        var total = await rows.CountAsync(ct);
        var items = await rows.OrderBy(x => x.Title).ThenBy(x => x.Id).Skip((page - 1) * 25).Take(25).ToArrayAsync(ct);
        return new(items.Select(x => x.View()).ToArray(), page, total);
    }
    public async Task<DocumentDetail?> GetAsync(Guid organization, Guid id, CancellationToken ct)
    {
        RequireTransaction();
        var document = await db.Documents.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organization && x.Id == id, ct);
        if (document is null) return null;
        var revisions = await db.Revisions.AsNoTracking().Where(x => x.OrganizationId == organization && x.DocumentId == id)
            .OrderByDescending(x => x.Number).ToArrayAsync(ct);
        return new(document.View(), revisions.Select(x => x.View()).ToArray());
    }
    public async Task<DocumentFile?> FileAsync(Guid organization, Guid id, int revision, bool inspection,
        DateTimeOffset now, CancellationToken ct)
    {
        RequireTransaction();
        if (revision is < 1 or > 100) throw new ArgumentException("Versão inválida.");
        var metadata = await db.Revisions.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organization
            && x.DocumentId == id && x.Number == revision, ct);
        if (metadata is null) return null;
        if (!inspection && (metadata.ScanState != "accepted" || metadata.ScannedAtUtc > now
            || metadata.ScannedAtUtc < now.AddHours(-24))) throw new DocumentStateException();
        var bytes = await db.Originals.AsNoTracking().Where(x => x.OrganizationId == organization
            && x.DocumentId == id && x.Number == revision).Select(x => x.Bytes).SingleOrDefaultAsync(ct);
        if (bytes is null) return null;
        if (bytes.Length != metadata.Length || Convert.ToHexString(SHA256.HashData(bytes)) != metadata.Sha256)
            throw new InvalidOperationException("Integridade do original não confirmada.");
        return new(metadata.View(), bytes);
    }
    public static DocumentCommand Normalize(DocumentCommand request, bool hasFile)
    {
        var create = request.Action == "create";
        if (request.CommandId == Guid.Empty || request.ExpectedSessionVersion < 1 || request.DocumentId == Guid.Empty
            || create != (request.DocumentId is null) || (create ? request.ExpectedVersion is not null : request.ExpectedVersion is null or < 1)
            || request.Action is not ("create" or "revision" or "edit" or "archive" or "restore" or "scan")
            || hasFile != (request.Action is "create" or "revision")
            || (request.Action == "scan" ? request.Revision is null or < 1 or > 100 : request.Revision is not null))
            throw new ArgumentException("Comando inválido.");
        var value = request.Action is "create" or "edit"
            ? Document.Normalize(request.Value ?? throw new ArgumentException("Campos obrigatórios.")) : null;
        if (value is null && request.Value is not null) throw new ArgumentException("Campos inesperados.");
        return request with { Value = value };
    }
    public async Task<DocumentMutation?> ApplyAsync(Guid organization, Guid actor, DocumentCommand request,
        ValidatedAttachment? file, AttachmentInspection? scan, DateTimeOffset now, CancellationToken ct)
    {
        RequireTransaction(); request = Normalize(request, file is not null);
        if (request.Action is "create" or "revision" or "scan")
        {
            if (scan is null || scan.State is not ("accepted" or "quarantined" or "blocked")
                || string.IsNullOrWhiteSpace(scan.Scanner) || scan.Scanner.Length > 80) throw new ArgumentException("Inspeção inválida.");
            if (file is not null && scan.State == "blocked") throw new DocumentStateException();
        }
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        { schema = 1, request, file = file is null ? null : new { file.FileName, file.MediaType, file.Sha256, length = file.Bytes.Length } })));
        var prior = await db.Receipts.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organization
            && x.ActorId == actor && x.CommandId == request.CommandId, ct);
        if (prior is not null)
        {
            if (prior.RequestHash != hash) throw new DocumentConflictException();
            return new(prior.DocumentId, prior.AppliedVersion, prior.Revision, prior.ScanState, prior.CommandId, true);
        }
        Document document;
        if (request.Action == "create")
        {
            document = new(organization, Guid.NewGuid(), request.Value!, now); db.Documents.Add(document);
        }
        else
        {
            var current = await db.Documents.SingleOrDefaultAsync(x => x.OrganizationId == organization && x.Id == request.DocumentId, ct);
            if (current is null) return null;
            document = current;
            document.Change(request.Action, request.ExpectedVersion!.Value, request.Value, now);
        }
        var number = request.Revision ?? document.LatestRevision;
        if (file is not null)
        {
            db.Revisions.Add(new() { OrganizationId = organization, DocumentId = document.Id, Number = number,
                FileName = file.FileName, MediaType = file.MediaType, Length = file.Bytes.Length, Sha256 = file.Sha256,
                ScanState = scan!.State, Scanner = scan.Scanner, CreatedAtUtc = now, ScannedAtUtc = now });
            db.Originals.Add(new() { OrganizationId = organization, DocumentId = document.Id, Number = number, Bytes = file.Bytes });
        }
        else if (request.Action == "scan")
        {
            var revision = await db.Revisions.SingleOrDefaultAsync(x => x.OrganizationId == organization && x.DocumentId == document.Id && x.Number == number, ct);
            if (revision is null) return null;
            revision.ScanState = scan!.State; revision.Scanner = scan.Scanner; revision.ScannedAtUtc = now;
        }
        db.Receipts.Add(new() { OrganizationId = organization, ActorId = actor, CommandId = request.CommandId, DocumentId = document.Id,
            RequestHash = hash, AppliedVersion = document.Version, Revision = number, ScanState = scan?.State, CreatedAtUtc = now });
        await db.SaveChangesAsync(ct);
        return new(document.Id, document.Version, number, scan?.State, request.CommandId, false);
    }
}
