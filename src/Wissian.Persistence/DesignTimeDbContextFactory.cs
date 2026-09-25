using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Wissian.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WissianDbContext>
{
    public WissianDbContext CreateDbContext(string[] args)
    {
        // Model generation never needs a live database or a credential.
        return new(new DbContextOptionsBuilder<WissianDbContext>()
            .UseNpgsql("Host=127.0.0.1;Database=wissian_design_only", p =>
                p.SetPostgresVersion(18, 6).MigrationsHistoryTable("schema_history", "wissian"))
            .Options);
    }
}
