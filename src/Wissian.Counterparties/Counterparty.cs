using Wissian.Core.Modularity;

namespace Wissian.Counterparties;

public sealed record CounterpartyInput(string Name, string Kind, string Relation, string? Reference);
public sealed record CounterpartyView(Guid Id, string Name, string Kind, string Relation, string? Reference,
    bool IsArchived, long Version);

public sealed class Counterparty
{
    private Counterparty() { }
    public Counterparty(Guid id, Guid organizationId, CounterpartyInput input, DateTimeOffset now)
    {
        if (id == Guid.Empty || organizationId == Guid.Empty) throw new ArgumentException("Identificadores inválidos.");
        Id = id; OrganizationId = organizationId; CreatedAtUtc = now; UpdatedAtUtc = now;
        Set(input);
    }
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = "";
    public string Kind { get; private set; } = "";
    public string Relation { get; private set; } = "";
    public string? Reference { get; private set; }
    public bool IsArchived { get; private set; }
    public long Version { get; private set; } = 1;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static CounterpartyInput Normalize(CounterpartyInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var name = Text(input.Name, 160, true)!;
        if (input.Kind is not ("organization" or "person") || input.Relation is not ("customer" or "supplier" or "both" or "other"))
            throw new ArgumentException("Tipo ou relação inválidos.");
        return new(name, input.Kind, input.Relation, Text(input.Reference, 80, false));
    }
    private static string? Text(string? value, int limit, bool required)
    {
        var text = value?.Trim().Normalize();
        if (string.IsNullOrEmpty(text))
        {
            if (required) throw new ArgumentException("Nome obrigatório.");
            return null;
        }
        if (text.Length > limit || text.Any(char.IsControl)) throw new ArgumentException("Texto inválido.");
        return text;
    }
    private void Set(CounterpartyInput input)
    {
        var normalized = Normalize(input);
        Name = normalized.Name; Kind = normalized.Kind; Relation = normalized.Relation; Reference = normalized.Reference;
    }
    public void Change(CounterpartyInput input, long expectedVersion, DateTimeOffset now)
    {
        CheckVersion(expectedVersion);
        if (IsArchived) throw new CounterpartyStateException();
        Set(input); Version = checked(Version + 1); UpdatedAtUtc = now;
    }
    public void Archive(bool archived, long expectedVersion, DateTimeOffset now)
    {
        CheckVersion(expectedVersion);
        if (IsArchived == archived) throw new CounterpartyStateException();
        IsArchived = archived; Version = checked(Version + 1); UpdatedAtUtc = now;
    }
    private void CheckVersion(long expected)
    {
        if (expected < 1 || expected != Version) throw new CounterpartyConflictException();
    }
    public CounterpartyView View() => new(Id, Name, Kind, Relation, Reference, IsArchived, Version);
}

public sealed class CounterpartyConflictException : Exception;
public sealed class CounterpartyStateException : Exception;
public static class CounterpartyModule
{
    public static ModuleManifest Manifest { get; } = new("counterparties", new(1, 0, 0), "Wissian",
        [new("core", new(1, 0, 0))], [new("counterparties.read", "counterparties.read"), new("counterparties.write", "counterparties.write")]);
}
