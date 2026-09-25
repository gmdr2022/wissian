using Microsoft.EntityFrameworkCore;
using Wissian.Persistence;

namespace Wissian.Host.Identity;

public sealed class RuntimeSchemaCheck(IServiceScopeFactory scopes) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<WissianDbContext>();
        var unsafeRole = await db.Database.SqlQueryRaw<bool>("""
            SELECT current_user <> 'wissian_runtime'
                OR r.rolsuper OR r.rolbypassrls OR r.rolcreaterole OR r.rolcreatedb
                OR pg_has_role(current_user, 'wissian_migrator', 'MEMBER')
                OR EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'wissian' AND nspowner = r.oid)
                OR EXISTS (SELECT 1 FROM pg_class t JOIN pg_namespace n ON n.oid = t.relnamespace
                    WHERE n.nspname = 'wissian' AND t.relowner = r.oid)
                AS "Value" FROM pg_roles r WHERE r.rolname = current_user
            """).SingleAsync(cancellationToken);
        if (unsafeRole) throw new InvalidOperationException("O host aceita somente a credencial runtime sem privilégios de migração.");
        var applied = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).ToArray();
        if (!db.Database.GetMigrations().SequenceEqual(applied))
            throw new InvalidOperationException("Schema incompatível. Aplicar migrações pelo processo separado antes de iniciar o host.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
