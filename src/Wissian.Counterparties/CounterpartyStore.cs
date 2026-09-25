using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Wissian.Counterparties;

public sealed record CounterpartyCommand(Guid CommandId, long ExpectedSessionVersion, long? ExpectedVersion,
    string Action, CounterpartyInput? Value);
public sealed record CounterpartyMutation(Guid Id, long Version, Guid CommandId, bool Replayed);
public sealed record CounterpartyPage(IReadOnlyList<CounterpartyView> Items, int Page, int Total);

public sealed class CounterpartyStore(CounterpartiesDbContext db)
{
    // The reviewed host establishes authority, RLS scope and the shared transaction before entering here.
    private void RequireTransaction()
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("Operação requer transação autorizada.");
    }
    public async Task<CounterpartyPage> SearchAsync(Guid organization, string? query, string status, int page, CancellationToken ct)
    {
        RequireTransaction();
        query = query?.Trim();
        if (query?.Length > 100 || page is < 1 or > 10000 || status is not ("active" or "archived" or "all"))
            throw new ArgumentException("Filtro inválido.");
        var rows = db.Counterparties.AsNoTracking().Where(x => x.OrganizationId == organization);
        if (status != "all") rows = rows.Where(x => x.IsArchived == (status == "archived"));
        if (!string.IsNullOrEmpty(query))
        {
            var term = query.ToLowerInvariant();
            rows = rows.Where(x => x.Name.ToLower().Contains(term) || x.Reference != null && x.Reference.ToLower().Contains(term));
        }
        var total = await rows.CountAsync(ct);
        var result = await rows.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * 25).Take(25).ToArrayAsync(ct);
        return new(result.Select(x => x.View()).ToArray(), page, total);
    }
    public async Task<CounterpartyView?> GetAsync(Guid organization, Guid id, CancellationToken ct)
    {
        RequireTransaction();
        return (await db.Counterparties.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organization && x.Id == id, ct))?.View();
    }
    public async Task<CounterpartyMutation?> ApplyAsync(Guid organization, Guid actor, Guid? id,
        CounterpartyCommand request, DateTimeOffset now, CancellationToken ct)
    {
        RequireTransaction();
        if (request.CommandId == Guid.Empty || request.ExpectedSessionVersion < 1 || id == Guid.Empty)
            throw new ArgumentException("Comando inválido.");
        var create = request.Action == "create";
        if (request.Action is not ("create" or "edit" or "archive" or "restore") || create != (id is null)
            || (create ? request.ExpectedVersion is not null : request.ExpectedVersion is null or < 1))
            throw new ArgumentException("Ação inválida.");
        CounterpartyInput? value = request.Action is "create" or "edit"
            ? Counterparty.Normalize(request.Value ?? throw new ArgumentException("Cadastro obrigatório.")) : null;
        if (value is null && request.Value is not null) throw new ArgumentException("Campos inesperados.");
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        { schema = 1, id, request.Action, request.ExpectedVersion, request.ExpectedSessionVersion, value })));
        var receipt = await db.Receipts.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organization
            && x.ActorId == actor && x.CommandId == request.CommandId, ct);
        if (receipt is not null)
        {
            if (receipt.RequestHash != hash) throw new CounterpartyConflictException();
            return new(receipt.CounterpartyId, receipt.AppliedVersion, receipt.CommandId, true);
        }
        Counterparty entity;
        if (create)
        {
            entity = new(Guid.NewGuid(), organization, value!, now);
            db.Counterparties.Add(entity);
        }
        else
        {
            var current = await db.Counterparties.SingleOrDefaultAsync(x => x.OrganizationId == organization && x.Id == id, ct);
            if (current is null) return null;
            entity = current;
            if (request.Action == "edit") entity.Change(value!, request.ExpectedVersion!.Value, now);
            else entity.Archive(request.Action == "archive", request.ExpectedVersion!.Value, now);
        }
        db.Receipts.Add(new() { OrganizationId = organization, ActorId = actor, CommandId = request.CommandId,
            CounterpartyId = entity.Id, RequestHash = hash, AppliedVersion = entity.Version, CreatedAtUtc = now });
        await db.SaveChangesAsync(ct);
        return new(entity.Id, entity.Version, request.CommandId, false);
    }
}
