using Wissian.Core.Modularity;

namespace Wissian.Documents;

public sealed record DocumentInput(string Title, string Classification, string? Source);
public sealed record DocumentView(Guid Id, string Title, string Classification, string? Source, bool IsArchived,
    long Version, int LatestRevision);
public sealed record RevisionView(int Number, string FileName, string MediaType, int Length, string Sha256,
    string ScanState, DateTimeOffset CreatedAtUtc, DateTimeOffset ScannedAtUtc);
public sealed record DocumentDetail(DocumentView Document, IReadOnlyList<RevisionView> Revisions);

public sealed class Document
{
    private Document() { }
    public Document(Guid organization, Guid id, DocumentInput input, DateTimeOffset now)
    {
        if (organization == Guid.Empty || id == Guid.Empty) throw new ArgumentException("Identificadores inválidos.");
        OrganizationId = organization; Id = id; CreatedAtUtc = now; UpdatedAtUtc = now; Set(input);
    }
    public Guid OrganizationId { get; private set; }
    public Guid Id { get; private set; }
    public string Title { get; private set; } = "";
    public string Classification { get; private set; } = "";
    public string? Source { get; private set; }
    public bool IsArchived { get; private set; }
    public long Version { get; private set; } = 1;
    public int LatestRevision { get; private set; } = 1;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static DocumentInput Normalize(DocumentInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Classification is not ("internal" or "restricted")) throw new ArgumentException("Classificação inválida.");
        return new(Text(input.Title, 160, true)!, input.Classification, Text(input.Source, 240, false));
    }
    private static string? Text(string? value, int limit, bool required)
    {
        var normalized = value?.Trim().Normalize();
        if (string.IsNullOrEmpty(normalized)) return required ? throw new ArgumentException("Texto obrigatório.") : null;
        if (normalized.Length > limit || normalized.Any(char.IsControl)) throw new ArgumentException("Texto inválido.");
        return normalized;
    }
    private void Set(DocumentInput input)
    {
        var normalized = Normalize(input);
        Title = normalized.Title; Classification = normalized.Classification; Source = normalized.Source;
    }
    public void Change(string action, long expected, DocumentInput? input, DateTimeOffset now)
    {
        if (expected != Version) throw new DocumentConflictException();
        if (action != "scan" && (action == "restore" ? !IsArchived : IsArchived)) throw new DocumentStateException();
        switch (action)
        {
            case "edit": Set(input ?? throw new ArgumentException("Campos obrigatórios.")); break;
            case "archive": IsArchived = true; break;
            case "restore": IsArchived = false; break;
            case "revision":
                if (LatestRevision >= 100) throw new DocumentLimitException();
                LatestRevision++; break;
            case "scan": break;
            default: throw new ArgumentException("Ação inválida.");
        }
        Version = checked(Version + 1); UpdatedAtUtc = now;
    }
    public DocumentView View() => new(Id, Title, Classification, Source, IsArchived, Version, LatestRevision);
}

public sealed class DocumentRevision
{
    public Guid OrganizationId { get; init; }
    public Guid DocumentId { get; init; }
    public int Number { get; init; }
    public string FileName { get; init; } = "";
    public string MediaType { get; init; } = "";
    public int Length { get; init; }
    public string Sha256 { get; init; } = "";
    public string ScanState { get; set; } = "quarantined";
    public string Scanner { get; set; } = "";
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset ScannedAtUtc { get; set; }
    public RevisionView View() => new(Number, FileName, MediaType, Length, Sha256, ScanState, CreatedAtUtc, ScannedAtUtc);
}

// Separate from metadata: lists never materialize original bytes. No public/static storage paths.
public sealed class DocumentOriginal
{
    public Guid OrganizationId { get; init; }
    public Guid DocumentId { get; init; }
    public int Number { get; init; }
    public byte[] Bytes { get; init; } = [];
}

public sealed class DocumentConflictException : Exception;
public sealed class DocumentStateException : Exception;
public sealed class DocumentLimitException : Exception;
public static class DocumentModule
{
    public static ModuleManifest Manifest { get; } = new("documents", new(1, 0, 0), "Wissian", [new("core", new(1, 0, 0))],
        [new("documents.read", "documents.read"), new("documents.write", "documents.write"), new("documents.restricted", "documents.restricted")]);
}
