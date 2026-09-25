using System.Text.Json;
using ClubAL.Domain.Audit;
using ClubAL.Domain.IdentityAccess;
using ClubAL.Domain.Tenancy;

// Synthetic, deterministic characterization of unchanged source fragments.
// No API, database, authentication provider, or tenant authorization is simulated.
var now = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
var tenantA = Guid.Parse("10000000-0000-0000-0000-000000000001");
var tenantB = Guid.Parse("10000000-0000-0000-0000-000000000002");
var user = Guid.Parse("20000000-0000-0000-0000-000000000001");
var membership = Guid.Parse("30000000-0000-0000-0000-000000000001");
var unit = Guid.Parse("40000000-0000-0000-0000-000000000001");
var results = new List<object>();
var failures = 0;

HumanSession Session(bool mfa = false) => new(
    Guid.Parse("50000000-0000-0000-0000-000000000001"), user,
    now, now.AddHours(1), mfa ? now : null);

void Run(string id, Action action)
{
    try
    {
        action();
        results.Add(new { id, status = "PASS" });
    }
    catch (Exception error)
    {
        failures++;
        results.Add(new { id, status = "FAIL", error = error.GetType().Name, message = error.Message });
    }
}

Run("M0-01-tenant-normalization", () => {
    var a = new Tenant(tenantA, " FULTUMA-EXEMPLO ", "Fultuma Exemplo", DeploymentEnvironment.Demonstration, now);
    var b = new Tenant(tenantB, "empresa-exemplo", "Empresa Exemplo", DeploymentEnvironment.Demonstration, now);
    Check(a.Slug == "fultuma-exemplo" && a.Id != b.Id && a.Version == 1, "Tenant identity contract");
});
Run("M0-02-tenant-invalid-input", () => {
    Throws<ArgumentException>(() => new Tenant(Guid.Empty, "a", "A", DeploymentEnvironment.Demonstration, now));
    Throws<ArgumentException>(() => new Tenant(tenantA, " ", "A", DeploymentEnvironment.Demonstration, now));
    Throws<ArgumentOutOfRangeException>(() => new Tenant(tenantA, new string('a', 81), "A", DeploymentEnvironment.Demonstration, now));
});
Run("M0-03-membership-references", () => {
    var link = new Membership(membership, user, tenantA, now);
    Check(link.TenantId == tenantA && link.UserId == user && link.IsActive, "Membership scope retained");
    Throws<ArgumentException>(() => new Membership(membership, user, Guid.Empty, now));
    Throws<ArgumentException>(() => new MembershipUnit(tenantA, Guid.Empty, unit));
});
Run("M0-04-session-invalid-lifetime", () => {
    Throws<ArgumentOutOfRangeException>(() => new HumanSession(membership, user, now, now));
    Throws<ArgumentException>(() => new HumanSession(Guid.Empty, user, now, now.AddHours(1)));
});
Run("M0-05-session-time-boundaries", () => {
    var session = Session();
    Check(!session.IsActiveAt(now.AddTicks(-1)), "Future session denied");
    Check(session.IsActiveAt(now), "Created session active");
    Check(!session.IsActiveAt(now.AddHours(1)), "Expiration exclusive");
});
Run("M0-06-context-idempotency", () => {
    var session = Session();
    session.SelectContext(tenantA, membership, unit, null, now);
    var version = session.Version;
    session.SelectContext(tenantA, membership, unit, null, now.AddMinutes(1));
    Check(session.Version == version && session.TenantId == tenantA, "Repeated context has no extra revision");
});
Run("M0-07-context-identifiers", () => {
    Throws<ArgumentException>(() => Session().SelectContext(Guid.Empty, membership, unit, null, now));
    Throws<ArgumentException>(() => Session().SelectContext(tenantA, membership, unit, Guid.Empty, now));
});
Run("M0-08-revocation-idempotency", () => {
    var session = Session();
    session.Revoke(now.AddMinutes(1));
    var version = session.Version;
    session.Revoke(now.AddMinutes(2));
    Check(!session.IsActiveAt(now.AddMinutes(3)) && session.Version == version && session.RevokedAtUtc == now.AddMinutes(1), "Revocation retains first evidence");
});
Run("M0-09-revoked-context-denied", () => {
    var session = Session();
    session.Revoke(now);
    Throws<InvalidOperationException>(() => session.SelectContext(tenantA, membership, unit, null, now.AddMinutes(1)));
});
Run("M0-10-platform-clears-tenant-context", () => {
    var session = Session(mfa: true);
    session.SelectContext(tenantA, membership, unit, null, now);
    session.EnterPlatform(membership, 1, now.AddMinutes(1), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    Check(session.TenantId is null && session.CurrentMembershipId is null && session.IsPlatformWindowActiveAt(now.AddMinutes(1), TimeSpan.FromMinutes(5)), "Platform context separated");
});
Run("M0-11-platform-requires-fresh-mfa", () => {
    Throws<InvalidOperationException>(() => Session().EnterPlatform(membership, 1, now, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5)));
    Throws<InvalidOperationException>(() => Session(true).EnterPlatform(membership, 1, now.AddMinutes(5), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5)));
});
Run("M0-12-platform-limits", () => {
    Throws<ArgumentOutOfRangeException>(() => Session(true).EnterPlatform(membership, 1, now, TimeSpan.FromMinutes(16), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5)));
});
Run("M0-13-platform-exit-on-context", () => {
    var session = Session(true);
    session.EnterPlatform(membership, 1, now, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    session.SelectContext(tenantA, membership, unit, null, now.AddMinutes(1));
    Check(!session.IsPlatformWindowActiveAt(now.AddMinutes(1), TimeSpan.FromMinutes(5)) && session.PlatformEndReasonCode == "context_changed", "Context change ends privileged window");
});
Run("M0-14-mfa-cannot-come-from-future", () => {
    Throws<ArgumentOutOfRangeException>(() => new HumanSession(membership, user, now, now.AddHours(1), now.AddHours(2)));
});
Run("M0-15-audit-keeps-scope", () => {
    var a = new AuditEvent(membership, now, "human", user, "test.read", "synthetic", "denied", "m0-a", tenantA);
    var b = new AuditEvent(unit, now, "human", user, "test.read", "synthetic", "denied", "m0-b", tenantB);
    Check(a.TenantId == tenantA && b.TenantId == tenantB && a.CorrelationId != b.CorrelationId, "Audit preserves origin scope");
});
Run("M0-16-audit-requires-correlation", () => {
    Throws<ArgumentException>(() => new AuditEvent(membership, now, "human", user, "test.read", "synthetic", "denied", " ", tenantA));
});

Console.WriteLine(JsonSerializer.Serialize(new {
    scope = "M0 domain characterization only", runtime = Environment.Version.ToString(),
    total = results.Count, passed = results.Count - failures, failed = failures,
    database_tested = false, api_tested = false, ui_tested = false,
    product_tenant_isolation_tested = false, results
}, new JsonSerializerOptions { WriteIndented = true }));
return failures == 0 ? 0 : 1;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}");
}
