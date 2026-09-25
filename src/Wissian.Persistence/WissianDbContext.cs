using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wissian.Core.Audit;
using Wissian.Core.Identity;
using Wissian.Core.Modularity;
using Wissian.Core.Organizations;

namespace Wissian.Persistence;

public sealed class WissianDbContext(DbContextOptions<WissianDbContext> options)
    : IdentityUserContext<WissianUser, Guid>(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<HumanSession> Sessions => Set<HumanSession>();
    public DbSet<ContextSelectionReceipt> ContextReceipts => Set<ContextSelectionReceipt>();
    public DbSet<OrganizationModule> OrganizationModules => Set<OrganizationModule>();
    public DbSet<MembershipPermission> Permissions => Set<MembershipPermission>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        model.HasDefaultSchema("wissian");
        model.Entity<WissianUser>(e =>
        {
            e.ToTable("identity_users");
            e.Property(p => p.DisplayName).HasMaxLength(160);
            e.Property(p => p.Id).ValueGeneratedNever();
        });
        model.Entity<IdentityUserClaim<Guid>>().ToTable("identity_claims");
        model.Entity<IdentityUserLogin<Guid>>().ToTable("identity_logins");
        model.Entity<IdentityUserToken<Guid>>().ToTable("identity_tokens");
        model.Entity<Organization>(e =>
        {
            e.ToTable("organizations");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).ValueGeneratedNever();
            e.Property(p => p.Slug).HasMaxLength(80);
            e.Property(p => p.DisplayName).HasMaxLength(160);
            e.HasIndex(p => p.Slug).IsUnique();
            e.Property(p => p.Version).IsConcurrencyToken();
        });
        model.Entity<Membership>(e =>
        {
            e.ToTable("memberships");
            e.HasKey(p => new { p.OrganizationId, p.Id });
            e.Property(p => p.Id).ValueGeneratedNever();
            e.HasAlternateKey(p => new { p.OrganizationId, p.Id, p.UserId });
            e.HasIndex(p => new { p.OrganizationId, p.UserId }).IsUnique();
            e.HasOne<Organization>().WithMany().HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<WissianUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
            e.Property(p => p.Version).IsConcurrencyToken();
        });
        model.Entity<HumanSession>(e =>
        {
            e.ToTable("sessions", t =>
            {
                t.HasCheckConstraint("sessions_context_complete", "(\"OrganizationId\" IS NULL AND \"MembershipId\" IS NULL AND \"MembershipVersion\" IS NULL) OR (\"OrganizationId\" IS NOT NULL AND \"MembershipId\" IS NOT NULL AND \"MembershipVersion\" IS NOT NULL AND \"MembershipVersion\" > 0)");
                t.HasCheckConstraint("sessions_lifetime", "\"ExpiresAtUtc\" > \"CreatedAtUtc\"");
            });
            e.HasKey(p => p.Id);
            e.HasAlternateKey(p => new { p.Id, p.UserId });
            e.Property(p => p.Id).ValueGeneratedNever();
            e.HasOne<WissianUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Membership>().WithMany()
                .HasForeignKey(p => new { p.OrganizationId, p.MembershipId, p.UserId })
                .HasPrincipalKey(p => new { p.OrganizationId, p.Id, p.UserId }).OnDelete(DeleteBehavior.Restrict);
            e.Property(p => p.Version).IsConcurrencyToken();
        });
        model.Entity<ContextSelectionReceipt>(e =>
        {
            e.ToTable("context_receipts", t => t.HasCheckConstraint("context_receipts_versions", "\"AppliedVersion\" > 0 AND \"MembershipVersion\" > 0"));
            e.HasKey(p => new { p.SessionId, p.CommandId });
            e.Property(p => p.RequestHash).HasMaxLength(64).IsFixedLength();
            e.HasOne<HumanSession>().WithMany().HasForeignKey(p => new { p.SessionId, p.UserId })
                .HasPrincipalKey(p => new { p.Id, p.UserId }).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Membership>().WithMany().HasForeignKey(p => new { p.OrganizationId, p.MembershipId, p.UserId })
                .HasPrincipalKey(p => new { p.OrganizationId, p.Id, p.UserId }).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<OrganizationModule>(e =>
        {
            e.ToTable("organization_modules");
            e.HasKey(p => new { p.OrganizationId, p.ModuleId });
            e.Property(p => p.ModuleId).HasMaxLength(80);
            e.Property(p => p.Version).IsConcurrencyToken();
            e.HasOne<Organization>().WithMany().HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<MembershipPermission>(e =>
        {
            e.ToTable("membership_permissions");
            e.HasKey(p => new { p.OrganizationId, p.MembershipId, p.Permission });
            e.Property(p => p.Permission).HasMaxLength(120);
            e.Property(p => p.Version).IsConcurrencyToken();
            e.HasOne<Membership>().WithMany().HasForeignKey(p => new { p.OrganizationId, p.MembershipId }).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<AuditEvent>(e =>
        {
            e.ToTable("audit_events");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).ValueGeneratedNever();
            e.Property(p => p.Action).HasMaxLength(120);
            e.Property(p => p.ResourceType).HasMaxLength(80);
            e.Property(p => p.ResourceId).HasMaxLength(120);
            e.Property(p => p.Outcome).HasMaxLength(40);
            e.Property(p => p.CorrelationId).HasMaxLength(120);
            e.HasIndex(p => new { p.OrganizationId, p.OccurredAtUtc });
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ProtectChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ProtectChanges();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    private void ProtectChanges()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is WissianUser user && entry.State == EntityState.Modified
                && entry.Property(nameof(WissianUser.IsActive)).IsModified
                && !Equals(entry.Property(nameof(WissianUser.IsActive)).OriginalValue, user.IsActive))
            {
                // Identity's store marks all fields modified, including unchanged IsActive. Only an
                // actual tracked transition invalidates tickets; a failed MFA attempt must not do so.
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.ConcurrencyStamp = Guid.NewGuid().ToString();
            }
            if (entry.Entity is AuditEvent && entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException("Auditoria não pode ser reescrita pelo runtime.");
            if (entry.Entity is ContextSelectionReceipt && entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException("Recibo concluído não pode ser reescrito pelo runtime.");
            if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("Version") is not null)
                entry.Property("Version").CurrentValue = checked((long)entry.Property("Version").OriginalValue! + 1);
        }
    }
}
