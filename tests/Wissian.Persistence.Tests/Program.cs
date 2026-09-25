using System.Data.Common;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wissian.Core.Audit;
using Wissian.Core.Identity;
using Wissian.Core.Modularity;
using Wissian.Core.Organizations;
using Wissian.Persistence;

var admin = Environment.GetEnvironmentVariable("WISSIAN_TEST_ADMIN_CONNECTION")
    ?? throw new InvalidOperationException("Executar por scripts/Test-Postgres.ps1, em cluster sintético novo.");
var adminOptions = new NpgsqlConnectionStringBuilder(admin);
if (adminOptions.Host != "127.0.0.1" || adminOptions.Username != "wissian_test_admin" || adminOptions.Database != "postgres")
    throw new InvalidOperationException("Este harness aceita apenas o administrador sintético em loopback.");
var migratorPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
var runtimePassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
await using var adminDb = new NpgsqlConnection(admin);
await adminDb.OpenAsync();
await Execute(adminDb, $"CREATE ROLE wissian_migrator LOGIN PASSWORD '{migratorPassword}' NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS");
await Execute(adminDb, $"CREATE ROLE wissian_runtime LOGIN PASSWORD '{runtimePassword}' NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS");
await Execute(adminDb, "CREATE DATABASE wissian_m1_synthetic OWNER wissian_migrator");
await Execute(adminDb, "REVOKE ALL ON DATABASE wissian_m1_synthetic FROM PUBLIC");
await Execute(adminDb, "GRANT CONNECT ON DATABASE wissian_m1_synthetic TO wissian_runtime");

string Connection(string user, string database = "wissian_m1_synthetic") => new NpgsqlConnectionStringBuilder(admin)
{
    Database = database, Username = user,
    Password = user == "wissian_migrator" ? migratorPassword : runtimePassword, Pooling = true,
    MaxPoolSize = 1, MinPoolSize = 0, NoResetOnClose = false, IncludeErrorDetail = false
}.ConnectionString;
var migrator = Connection("wissian_migrator");
var runtime = Connection("wissian_runtime");
WissianDbContext Db(string connection, bool failMigration = false)
{
    var options = new DbContextOptionsBuilder<WissianDbContext>().UseNpgsql(connection,
        p => p.SetPostgresVersion(18, 6).MigrationsHistoryTable("schema_history", "wissian"));
    if (failMigration) options.AddInterceptors(new FailingMigration());
    return new(options.Options);
}

static async Task Execute(NpgsqlConnection connection, string sql)
{
    await using var command = new NpgsqlCommand(sql, connection);
    await command.ExecuteNonQueryAsync();
}
static void Require(bool success, string message = "Oráculo não atendido")
{
    if (!success) throw new InvalidOperationException(message);
}
static async Task PgDenied(Func<Task> action, string sqlState = "42501")
{
    try { await action(); }
    catch (PostgresException error) when (error.SqlState == sqlState) { return; }
    catch (DbUpdateException error) when (error.InnerException is PostgresException pg && pg.SqlState == sqlState) { return; }
    throw new InvalidOperationException($"Era esperado SQLSTATE {sqlState}.");
}
var tests = new List<(string Name, Func<Task> Run)>();
void Test(string name, Func<Task> run) => tests.Add((name, run));
var now = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
var actorA = Guid.NewGuid();
var actorB = Guid.NewGuid();
var administrator = Guid.NewGuid();
var orgA = new Organization(Guid.NewGuid(), "fultuma-exemplo", "Fultuma Exemplo", now);
var orgB = new Organization(Guid.NewGuid(), "empresa-exemplo", "Empresa Exemplo", now);
var memberA = new Membership(Guid.NewGuid(), actorA, orgA.Id, now);
var memberB = new Membership(Guid.NewGuid(), actorB, orgB.Id, now);
var sessionA = new HumanSession(Guid.NewGuid(), actorA, now, now.AddHours(1));
var sessionB = new HumanSession(Guid.NewGuid(), actorB, now, now.AddHours(1));
sessionA.SelectOrganization(memberA, orgA, now);
sessionB.SelectOrganization(memberB, orgB, now);

Test("M1-S2-01 new migration and repeat application", async () =>
{
    await using var db = Db(migrator);
    await db.Database.MigrateAsync();
    await db.Database.MigrateAsync();
    Require((await db.Database.GetAppliedMigrationsAsync()).SequenceEqual(db.Database.GetMigrations()));
    db.Users.AddRange(new[] { actorA, actorB, administrator }.Select(id => new WissianUser
    {
        Id = id, UserName = id + "@example.invalid", NormalizedUserName = id.ToString().ToUpperInvariant() + "@EXAMPLE.INVALID",
        DisplayName = "Pessoa sintética", IsActive = true, CreatedAtUtc = now
    }));
    db.Organizations.AddRange(orgA, orgB);
    db.Memberships.AddRange(memberA, memberB);
    db.Sessions.AddRange(sessionA, sessionB);
    db.OrganizationModules.AddRange(new OrganizationModule(orgA.Id, "foundation-probe", true, true), new OrganizationModule(orgB.Id, "foundation-probe", true, true));
    db.Permissions.AddRange(new MembershipPermission(orgA.Id, memberA.Id, "foundation-probe.inspect"), new MembershipPermission(orgB.Id, memberB.Id, "foundation-probe.inspect"));
    await db.SaveChangesAsync();
});
Test("M1-S2-02 runtime cannot own schema, bypass RLS, or become migrator", async () =>
{
    await using var db = Db(runtime);
    Require(!await db.Database.SqlQueryRaw<bool>("SELECT rolsuper OR rolbypassrls OR rolcreaterole AS \"Value\" FROM pg_roles WHERE rolname = current_user").SingleAsync());
    Require(!await db.Database.SqlQueryRaw<bool>("SELECT pg_has_role(current_user, 'wissian_migrator', 'MEMBER') AS \"Value\"").SingleAsync());
    await PgDenied(() => db.Database.ExecuteSqlRawAsync("CREATE TABLE wissian.forbidden_table (id integer)"));
    await PgDenied(() => db.Database.ExecuteSqlRawAsync("ALTER TABLE wissian.organizations DISABLE ROW LEVEL SECURITY"));
    await PgDenied(() => db.Database.ExecuteSqlRawAsync("SET ROLE wissian_migrator"));
    var stolenRole = new NpgsqlConnectionStringBuilder(runtime) { Username = "wissian_migrator" };
    await using var forbiddenConnection = new NpgsqlConnection(stolenRole.ConnectionString);
    await PgDenied(() => forbiddenConnection.OpenAsync(), "28P01");
});
Test("M1-S2-03 pool A then B then no scope reuses connection without context", async () =>
{
    var pids = new List<int>();
    foreach (var pair in new[] { (actorA, orgA.Id), (actorB, orgB.Id) })
    {
        await using var db = Db(runtime);
        await using var transaction = await ScopedTransaction.BeginAsync(db, pair.Item1, pair.Item2);
        pids.Add(await db.Database.SqlQueryRaw<int>("SELECT pg_backend_pid() AS \"Value\"").SingleAsync());
        Require((await db.Organizations.Select(o => o.Id).ToListAsync()).SequenceEqual([pair.Item2]));
        Require(await db.Memberships.CountAsync() == 1 && await db.OrganizationModules.CountAsync() == 1);
        await transaction.CommitAsync();
    }
    await using var unscoped = Db(runtime);
    pids.Add(await unscoped.Database.SqlQueryRaw<int>("SELECT pg_backend_pid() AS \"Value\"").SingleAsync());
    Require(pids.Distinct().Count() == 1, "O teste deve exercitar a mesma conexão física.");
    Require(await unscoped.Organizations.CountAsync() == 0 && await unscoped.Memberships.CountAsync() == 0);
    Require(await unscoped.Sessions.CountAsync() == 0 && await unscoped.OrganizationModules.CountAsync() == 0);
});
Test("M1-S2-04 foreign organization and platform actor have no membership authority", async () =>
{
    foreach (var actor in new[] { actorA, administrator })
    {
        await using var db = Db(runtime);
        await using var transaction = await ScopedTransaction.BeginAsync(db, actor, orgB.Id);
        Require(await db.Organizations.CountAsync() == 0 && await db.Memberships.CountAsync() == 0);
        Require(await db.OrganizationModules.CountAsync() == 0 && await db.Permissions.CountAsync() == 0);
    }
});
Test("M1-S2-05 unselected actor can discover only its own organization", async () =>
{
    await using var db = Db(runtime);
    await using var transaction = await ScopedTransaction.BeginAsync(db, actorA);
    Require((await db.Organizations.Select(o => o.Id).ToListAsync()).SequenceEqual([orgA.Id]));
    Require(await db.OrganizationModules.CountAsync() == 0 && await db.Permissions.CountAsync() == 0);
});
Test("M1-S2-06 runtime cross-organization FK cannot attach membership", async () =>
{
    await using var db = Db(runtime);
    await using var transaction = await ScopedTransaction.BeginAsync(db, actorA, orgA.Id);
    await PgDenied(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE wissian.sessions SET \"OrganizationId\" = {orgB.Id} WHERE \"Id\" = {sessionA.Id}"), "23503");
});
Test("M1-S2-07 foreign session insert is blocked by RLS", async () =>
{
    await using var db = Db(runtime);
    await using var transaction = await ScopedTransaction.BeginAsync(db, actorA, orgA.Id);
    db.Sessions.Add(new HumanSession(Guid.NewGuid(), actorB, now, now.AddHours(1)));
    await PgDenied(() => db.SaveChangesAsync());
});
Test("M1-S2-08 audit scope cannot be forged and history cannot be rewritten", async () =>
{
    await using (var db = Db(runtime))
    {
        await using var transaction = await ScopedTransaction.BeginAsync(db, actorA, orgA.Id);
        db.AuditEvents.Add(new AuditEvent(Guid.NewGuid(), now, actorA, orgA.Id, "session.test", "session", sessionA.Id.ToString(), "allowed", "synthetic-audit", 1));
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    await using (var db = Db(runtime))
    {
        await using var transaction = await ScopedTransaction.BeginAsync(db, actorA, orgA.Id);
        Require(await db.AuditEvents.CountAsync() == 1);
        await PgDenied(() => db.Database.ExecuteSqlRawAsync("DELETE FROM wissian.audit_events"));
    }
    await using (var db = Db(runtime))
    {
        await using var transaction = await ScopedTransaction.BeginAsync(db, actorA, orgA.Id);
        db.AuditEvents.Add(new AuditEvent(Guid.NewGuid(), now, actorA, orgB.Id, "session.test", "session", null, "allowed", "synthetic-cross-scope", 1));
        await PgDenied(() => db.SaveChangesAsync());
    }
});
Test("M1-S2-09 runtime cannot mint a membership permission", async () =>
{
    await using var db = Db(runtime);
    await using var transaction = await ScopedTransaction.BeginAsync(db, actorA, orgA.Id);
    db.Permissions.Add(new MembershipPermission(orgA.Id, memberA.Id, "foundation-probe.elevated"));
    await PgDenied(() => db.SaveChangesAsync());
});
Test("M1-S2-10 two stale session updates surface concurrency conflict", async () =>
{
    await using var first = Db(runtime);
    await using var second = Db(runtime);
    HumanSession firstSession, secondSession;
    await using (var tx = await ScopedTransaction.BeginAsync(first, actorA)) firstSession = await first.Sessions.SingleAsync(s => s.Id == sessionA.Id);
    await using (var tx = await ScopedTransaction.BeginAsync(second, actorA)) secondSession = await second.Sessions.SingleAsync(s => s.Id == sessionA.Id);
    firstSession.Revoke(now.AddMinutes(1));
    await using (var tx = await ScopedTransaction.BeginAsync(first, actorA))
    {
        await first.SaveChangesAsync();
        await tx.CommitAsync();
    }
    secondSession.Revoke(now.AddMinutes(2));
    await using var secondTx = await ScopedTransaction.BeginAsync(second, actorA);
    try { await second.SaveChangesAsync(); }
    catch (DbUpdateConcurrencyException) { return; }
    throw new InvalidOperationException("Gravação obsoleta não foi recusada.");
});
Test("M1-S2-11 membership revocation denies next scoped query", async () =>
{
    await using (var owner = Db(migrator))
    {
        (await owner.Memberships.SingleAsync(m => m.Id == memberB.Id)).Revoke();
        await owner.SaveChangesAsync();
    }
    await using var db = Db(runtime);
    await using var tx = await ScopedTransaction.BeginAsync(db, actorB, orgB.Id);
    Require(await db.Organizations.CountAsync() == 0 && await db.OrganizationModules.CountAsync() == 0 && await db.Permissions.CountAsync() == 0);
});
Test("M1-S2-12 duplicate membership and incomplete session rejected by database", async () =>
{
    await using (var db = Db(migrator))
    {
        db.Memberships.Add(new Membership(Guid.NewGuid(), actorA, orgA.Id, now));
        await PgDenied(() => db.SaveChangesAsync(), "23505");
    }
    await using (var db = Db(runtime))
    {
        await using var tx = await ScopedTransaction.BeginAsync(db, actorB);
        await PgDenied(() => db.Database.ExecuteSqlInterpolatedAsync($"UPDATE wissian.sessions SET \"MembershipVersion\" = NULL WHERE \"Id\" = {sessionB.Id}"), "23514");
    }
});
Test("M1-S2-13 failing migration rolls back and resumes in separate database", async () =>
{
    await Execute(adminDb, "CREATE DATABASE wissian_m1_recovery_synthetic OWNER wissian_migrator");
    var recovery = Connection("wissian_migrator", "wissian_m1_recovery_synthetic");
    await using (var broken = Db(recovery, failMigration: true))
    {
        var faultObserved = false;
        try { await broken.Database.MigrateAsync(); }
        catch (InjectedMigrationFailure) { faultObserved = true; }
        Require(faultObserved);
    }
    await using var clean = Db(recovery);
    Require(await clean.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM information_schema.tables WHERE table_schema = 'wissian' AND table_name <> 'schema_history'").SingleAsync() == 0);
    await clean.Database.MigrateAsync();
    Require((await clean.Database.GetAppliedMigrationsAsync()).SequenceEqual(clean.Database.GetMigrations()));
    Require(await clean.Organizations.CountAsync() == 0);
});

Test("M1-S2-14 audit also rejects a forged matching database scope without membership", async () =>
{
    await using var db = Db(runtime);
    await using var tx = await ScopedTransaction.BeginAsync(db, actorA, orgB.Id);
    db.AuditEvents.Add(new AuditEvent(Guid.NewGuid(), now, actorA, orgB.Id, "scope.test", "organization", null, "denied", "forged-scope", 1));
    await PgDenied(() => db.SaveChangesAsync());
});

Test("M1-S2-15 additive migration preserves existing scoped audit and organization", async () =>
{
    await Execute(adminDb, "CREATE DATABASE wissian_m1_upgrade_synthetic OWNER wissian_migrator");
    await Execute(adminDb, "REVOKE ALL ON DATABASE wissian_m1_upgrade_synthetic FROM PUBLIC");
    await Execute(adminDb, "GRANT CONNECT ON DATABASE wissian_m1_upgrade_synthetic TO wissian_runtime");
    var upgrade = Connection("wissian_migrator", "wissian_m1_upgrade_synthetic");
    var organization = new Organization(Guid.NewGuid(), "upgrade-example", "Exemplo preservado", now);
    var auditId = Guid.NewGuid();
    await using (var baseline = Db(upgrade))
    {
        await baseline.GetService<IMigrator>().MigrateAsync("20260925152926_Foundation");
        baseline.Users.Add(new WissianUser { Id = actorA, DisplayName = "Pessoa sintética", IsActive = true, CreatedAtUtc = now });
        baseline.Organizations.Add(organization);
        baseline.Memberships.Add(new Membership(Guid.NewGuid(), actorA, organization.Id, now));
        baseline.AuditEvents.Add(new AuditEvent(auditId, now, actorA, organization.Id, "upgrade.test", "organization", organization.Id.ToString(), "allowed", "before-additive-migration", 1));
        await baseline.SaveChangesAsync();
        await baseline.Database.MigrateAsync();
        Require((await baseline.Database.GetAppliedMigrationsAsync()).SequenceEqual(baseline.Database.GetMigrations()));
    }
    await using var upgraded = Db(Connection("wissian_runtime", "wissian_m1_upgrade_synthetic"));
    await using var tx = await ScopedTransaction.BeginAsync(upgraded, actorA, organization.Id);
    Require((await upgraded.Organizations.SingleAsync()).DisplayName == "Exemplo preservado");
    var audit = await upgraded.AuditEvents.SingleAsync();
    Require(audit.Id == auditId && audit.CorrelationId == "before-additive-migration" && audit.AuthorityVersion == 1);
});
Test("M1-S2-16 global authentication audit works without granting organization access", async () =>
{
    await using var db = Db(runtime);
    await using var tx = await ScopedTransaction.BeginAsync(db, administrator);
    db.AuditEvents.Add(new AuditEvent(Guid.NewGuid(), now, administrator, null, "authentication.test", "session", null, "denied", "global-authentication", 1));
    await db.SaveChangesAsync();
    Require(await db.AuditEvents.CountAsync() == 1 && await db.Organizations.CountAsync() == 0);
    await tx.CommitAsync();
});

var failed = 0;
foreach (var test in tests)
{
    try { await test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception error) { failed++; Console.Error.WriteLine($"FAIL {test.Name}: {error.GetType().Name}: {error.Message}"); }
}
NpgsqlConnection.ClearAllPools();
Console.WriteLine($"Persistence: {tests.Count - failed}/{tests.Count} passed; {failed} failed; PostgreSQL 18.6, synthetic cluster only.");
return failed == 0 ? 0 : 1;

sealed class InjectedMigrationFailure : Exception;
sealed class FailingMigration : DbCommandInterceptor
{
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (command.CommandText.Contains("CREATE TABLE wissian.memberships", StringComparison.Ordinal))
            throw new InjectedMigrationFailure();
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }
}
