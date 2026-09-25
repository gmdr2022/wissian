using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Wissian.Counterparties;

public sealed class CounterpartyReceipt
{
    public Guid OrganizationId { get; init; }
    public Guid ActorId { get; init; }
    public Guid CommandId { get; init; }
    public Guid CounterpartyId { get; init; }
    public string RequestHash { get; init; } = "";
    public long AppliedVersion { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

// This model owns only this module's tables. Identity data stays behind the host authority adapter.
public sealed class CounterpartiesDbContext(DbContextOptions<CounterpartiesDbContext> options) : DbContext(options)
{
    public DbSet<Counterparty> Counterparties => Set<Counterparty>();
    public DbSet<CounterpartyReceipt> Receipts => Set<CounterpartyReceipt>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.HasDefaultSchema("wissian_counterparties");
        model.Entity<Counterparty>(e =>
        {
            e.ToTable("counterparties", t =>
            {
                t.HasCheckConstraint("counterparty_version", "\"Version\" > 0");
                t.HasCheckConstraint("counterparty_kind", "\"Kind\" IN ('organization', 'person')");
                t.HasCheckConstraint("counterparty_relation", "\"Relation\" IN ('customer', 'supplier', 'both', 'other')");
            });
            e.HasKey(x => new { x.OrganizationId, x.Id });
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(160);
            e.Property(x => x.Kind).HasMaxLength(20);
            e.Property(x => x.Relation).HasMaxLength(20);
            e.Property(x => x.Reference).HasMaxLength(80);
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasIndex(x => new { x.OrganizationId, x.IsArchived, x.Name, x.Id });
        });
        model.Entity<CounterpartyReceipt>(e =>
        {
            e.ToTable("command_receipts");
            e.HasKey(x => new { x.OrganizationId, x.ActorId, x.CommandId });
            e.Property(x => x.RequestHash).HasMaxLength(64).IsFixedLength();
            e.HasOne<Counterparty>().WithMany().HasForeignKey(x => new { x.OrganizationId, x.CounterpartyId }).OnDelete(DeleteBehavior.Restrict);
        });
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Deleted || entry.Entity is CounterpartyReceipt && entry.State == EntityState.Modified)
                throw new InvalidOperationException("Preservar cadastros e recibos. Use o arquivamento.");
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<CounterpartiesDbContext>
{
    public CounterpartiesDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<CounterpartiesDbContext>()
        .UseNpgsql("Host=127.0.0.1;Database=wissian_design_only", p => p.SetPostgresVersion(18, 6)
            .MigrationsHistoryTable("schema_history", "wissian_counterparties")).Options);
}
