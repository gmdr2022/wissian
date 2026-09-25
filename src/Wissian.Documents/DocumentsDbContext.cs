using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Wissian.Documents;

public sealed class DocumentReceipt
{
    public Guid OrganizationId { get; init; }
    public Guid ActorId { get; init; }
    public Guid CommandId { get; init; }
    public Guid DocumentId { get; init; }
    public string RequestHash { get; init; } = "";
    public long AppliedVersion { get; init; }
    public int Revision { get; init; }
    public string? ScanState { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public sealed class DocumentsDbContext(DbContextOptions<DocumentsDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentRevision> Revisions => Set<DocumentRevision>();
    public DbSet<DocumentOriginal> Originals => Set<DocumentOriginal>();
    public DbSet<DocumentReceipt> Receipts => Set<DocumentReceipt>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.HasDefaultSchema("wissian_documents");
        model.Entity<Document>(e =>
        {
            e.ToTable("documents", t =>
            {
                t.HasCheckConstraint("document_version", "\"Version\" > 0 AND \"LatestRevision\" BETWEEN 1 AND 100");
                t.HasCheckConstraint("document_classification", "\"Classification\" IN ('internal', 'restricted')");
            });
            e.HasKey(x => new { x.OrganizationId, x.Id });
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Title).HasMaxLength(160);
            e.Property(x => x.Classification).HasMaxLength(20);
            e.Property(x => x.Source).HasMaxLength(240);
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasIndex(x => new { x.OrganizationId, x.IsArchived, x.Title, x.Id });
        });
        model.Entity<DocumentRevision>(e =>
        {
            e.ToTable("revisions", t =>
            {
                t.HasCheckConstraint("revision_number", "\"Number\" BETWEEN 1 AND 100");
                t.HasCheckConstraint("revision_length", "\"Length\" BETWEEN 1 AND 10485760");
                t.HasCheckConstraint("revision_scan_state", "\"ScanState\" IN ('accepted', 'quarantined', 'blocked')");
            });
            e.HasKey(x => new { x.OrganizationId, x.DocumentId, x.Number });
            e.Property(x => x.FileName).HasMaxLength(180);
            e.Property(x => x.MediaType).HasMaxLength(80);
            e.Property(x => x.Sha256).HasMaxLength(64).IsFixedLength();
            e.Property(x => x.ScanState).HasMaxLength(20);
            e.Property(x => x.Scanner).HasMaxLength(80);
            e.HasOne<Document>().WithMany().HasForeignKey(x => new { x.OrganizationId, Id = x.DocumentId }).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<DocumentOriginal>(e =>
        {
            e.ToTable("originals", t => t.HasCheckConstraint("original_length", "octet_length(\"Bytes\") BETWEEN 1 AND 10485760"));
            e.HasKey(x => new { x.OrganizationId, x.DocumentId, x.Number });
            e.HasOne<DocumentRevision>().WithOne().HasForeignKey<DocumentOriginal>(x => new { x.OrganizationId, x.DocumentId, x.Number }).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<DocumentReceipt>(e =>
        {
            e.ToTable("command_receipts");
            e.HasKey(x => new { x.OrganizationId, x.ActorId, x.CommandId });
            e.Property(x => x.RequestHash).HasMaxLength(64).IsFixedLength();
            e.Property(x => x.ScanState).HasMaxLength(20);
            e.HasOne<Document>().WithMany().HasForeignKey(x => new { x.OrganizationId, Id = x.DocumentId }).OnDelete(DeleteBehavior.Restrict);
        });
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified && entry.Entity is DocumentOriginal or DocumentReceipt)
                throw new InvalidOperationException("Preservar originais e recibos.");
            if (entry.State == EntityState.Modified && entry.Entity is DocumentRevision
                && entry.Properties.Any(p => p.IsModified && p.Metadata.Name is not ("ScanState" or "Scanner" or "ScannedAtUtc")))
                throw new InvalidOperationException("Versão documental imutável.");
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<DocumentsDbContext>
{
    public DocumentsDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<DocumentsDbContext>()
        .UseNpgsql("Host=127.0.0.1;Database=wissian_design_only", p => p.SetPostgresVersion(18, 6)
            .MigrationsHistoryTable("schema_history", "wissian_documents")).Options);
}
