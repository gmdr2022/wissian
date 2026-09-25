using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Wissian.Core.Audit;
using Wissian.Core.Identity;
using Wissian.Core.Modularity;
using Wissian.Core.Organizations;
using Wissian.Host;
using Wissian.TestExtension;

var tests = new List<(string Name, Func<Task> Run)>();
var now = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
var userA = Guid.NewGuid();
var userB = Guid.NewGuid();
var organizationA = new Organization(Guid.NewGuid(), "fultuma-exemplo", "Fultuma Exemplo", now);
var organizationB = new Organization(Guid.NewGuid(), "empresa-exemplo", "Empresa Exemplo", now);
var catalog = new ModuleCatalog([FoundationProbe.Manifest]);
var enabled = Set("foundation-probe");
var permissions = Set("foundation-probe.inspect");

void Test(string name, Action run) => tests.Add((name, () => { run(); return Task.CompletedTask; }));
void AsyncTest(string name, Func<Task> run) => tests.Add((name, run));
static HashSet<string> Set(params string[] values) => new(values, StringComparer.Ordinal);
static void Require(bool condition, string message = "Oráculo não atendido")
{
    if (!condition) throw new InvalidOperationException(message);
}
static void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Era esperada {typeof(T).Name}.");
}
(HumanSession Session, Membership Membership) Context(Organization organization, Guid user)
{
    var membership = new Membership(Guid.NewGuid(), user, organization.Id, now);
    var session = new HumanSession(Guid.NewGuid(), user, now, now.AddHours(1));
    session.SelectOrganization(membership, organization, now);
    return (session, membership);
}
bool Allowed(HumanSession? session, Membership? membership, Organization? organization, Guid? user = null,
    bool active = true, HashSet<string>? eligible = null, HashSet<string>? activeModules = null,
    HashSet<string>? granted = null, DateTimeOffset? instant = null, string capability = "foundation-probe.inspect") =>
    OrganizationAccess.Allows(session, membership, organization, user ?? userA, active, instant ?? now,
        catalog, "foundation-probe", capability, eligible ?? enabled, activeModules ?? enabled, granted ?? permissions);

Test("M1-S1-01 organization normalization and no unit requirement", () =>
{
    var organization = new Organization(Guid.NewGuid(), " FULTUMA-EXEMPLO ", " Fultuma Exemplo ", now.ToOffset(TimeSpan.FromHours(-3)));
    Require(organization.Slug == "fultuma-exemplo" && organization.DisplayName == "Fultuma Exemplo");
    Require(organization.CreatedAtUtc.Offset == TimeSpan.Zero && organization.Version == 1);
});
Test("M1-S1-02 invalid identity and slug rejected", () =>
{
    Throws<ArgumentException>(() => new Organization(Guid.Empty, "valid", "Valid", now));
    Throws<ArgumentException>(() => new Organization(Guid.NewGuid(), "../other", "Valid", now));
    Throws<ArgumentException>(() => new Membership(Guid.NewGuid(), Guid.Empty, organizationA.Id, now));
    Throws<ArgumentOutOfRangeException>(() => new Organization(Guid.NewGuid(), "valid", new string('a', 161), now));
});
Test("M1-S1-03 session absolute lifetime boundaries", () =>
{
    var session = new HumanSession(Guid.NewGuid(), userA, now, now.AddMinutes(1));
    Require(!session.IsActiveAt(now.AddTicks(-1)) && session.IsActiveAt(now));
    Require(session.IsActiveAt(now.AddMinutes(1).AddTicks(-1)) && !session.IsActiveAt(now.AddMinutes(1)));
    Throws<ArgumentOutOfRangeException>(() => new HumanSession(Guid.NewGuid(), userA, now, now));
});
Test("M1-S1-04 context selection is idempotent", () =>
{
    var (session, membership) = Context(organizationA, userA);
    var version = session.Version;
    session.SelectOrganization(membership, organizationA, now);
    Require(session.Version == version && session.MembershipVersion == membership.Version);
});
Test("M1-S1-05 foreign membership or mismatched organization denied", () =>
{
    var (session, membership) = Context(organizationA, userA);
    var foreignMembership = new Membership(Guid.NewGuid(), userB, organizationB.Id, now);
    Throws<InvalidOperationException>(() => session.SelectOrganization(foreignMembership, organizationB, now));
    Throws<InvalidOperationException>(() => session.SelectOrganization(membership, organizationB, now));
    Require(!Allowed(session, foreignMembership, organizationB) && !Allowed(session, membership, organizationB));
});
Test("M1-S1-06 same policy supports two separate organizations", () =>
{
    var (sessionA, membershipA) = Context(organizationA, userA);
    var (sessionB, membershipB) = Context(organizationB, userB);
    Require(Allowed(sessionA, membershipA, organizationA));
    Require(Allowed(sessionB, membershipB, organizationB, userB));
    Require(!Allowed(sessionA, membershipB, organizationB));
    Require(!Allowed(sessionB, membershipA, organizationA, userB));
});
Test("M1-S1-07 missing context or platform label grants nothing", () =>
{
    var (_, membership) = Context(organizationA, userA);
    var unselected = new HumanSession(Guid.NewGuid(), userA, now, now.AddHours(1));
    Require(!Allowed(unselected, membership, organizationA, granted: Set("platform.admin", "foundation-probe.inspect")));
    Require(!Allowed(null, membership, organizationA));
    Require(!Allowed(unselected, null, null));
});
Test("M1-S1-08 revocation takes effect with existing session object", () =>
{
    var (session, membership) = Context(organizationA, userA);
    Require(Allowed(session, membership, organizationA));
    membership.Revoke();
    Require(!Allowed(session, membership, organizationA));
    var version = membership.Version;
    membership.Revoke();
    Require(membership.Version == version);
    Throws<InvalidOperationException>(() => session.SelectOrganization(membership, organizationA, now));
});
Test("M1-S1-09 session revocation clears context and stays revoked", () =>
{
    var (session, membership) = Context(organizationA, userA);
    session.Revoke(now);
    var version = session.Version;
    session.Revoke(now.AddMinutes(1));
    Require(session.Version == version && session.OrganizationId is null && session.MembershipId is null);
    Require(!Allowed(session, membership, organizationA));
    Throws<InvalidOperationException>(() => session.SelectOrganization(membership, organizationA, now));
});
Test("M1-S1-10 disabled organization, user, or expired session denied", () =>
{
    var organization = new Organization(Guid.NewGuid(), "temporary-example", "Exemplo", now);
    var (session, membership) = Context(organization, userA);
    Require(!Allowed(session, membership, organization, active: false));
    Require(!Allowed(session, membership, organization, instant: now.AddHours(1)));
    organization.Suspend();
    Require(!Allowed(session, membership, organization));
});
Test("M1-S1-11 disabled module denies despite permission", () =>
{
    var (session, membership) = Context(organizationA, userA);
    Require(Allowed(session, membership, organizationA));
    Require(!Allowed(session, membership, organizationA, activeModules: Set()));
});
Test("M1-S1-12 enabled module without entitlement or grant denied", () =>
{
    var (session, membership) = Context(organizationA, userA);
    Require(!Allowed(session, membership, organizationA, eligible: Set()));
    Require(!Allowed(session, membership, organizationA, granted: Set()));
    Require(!Allowed(session, membership, organizationA, granted: Set("*", "foundation-probe.*")));
    Require(!Allowed(session, membership, organizationA, capability: "foundation-probe.unknown"));
});
Test("M1-S1-13 audit carries scope and authority version", () =>
{
    var audit = new AuditEvent(Guid.NewGuid(), now, userA, organizationA.Id, "context.selected", "membership",
        Guid.NewGuid().ToString(), "allowed", "synthetic-correlation", 2);
    Require(audit.OrganizationId == organizationA.Id && audit.AuthorityVersion == 2 && audit.SchemaVersion == 1);
    Throws<ArgumentException>(() => new AuditEvent(Guid.NewGuid(), now, userA, null, "signout", "session", null, "allowed", " ", 1));
    Throws<ArgumentOutOfRangeException>(() => new AuditEvent(Guid.NewGuid(), now, userA, null, "signout", "session", null, "allowed", "test", 0));
});

ModuleManifest Manifest(string id, params ModuleDependency[] dependencies) => new(id, new(1, 0, 0), "Synthetic tests", dependencies, []);
Test("M1-S1-14 neutral catalog contains core only", () => Require(new ModuleCatalog([]).Modules.Keys.SequenceEqual(["core"])));
Test("M1-S1-15 missing module dependency blocks composition", () =>
    Throws<InvalidOperationException>(() => new ModuleCatalog([Manifest("alpha", new ModuleDependency("absent", new(1, 0, 0)))])));
Test("M1-S1-16 incompatible contract blocks composition", () =>
    Throws<InvalidOperationException>(() => new ModuleCatalog([Manifest("alpha", new ModuleDependency("core", new(2, 0, 0)))])));
Test("M1-S1-17 cycle blocks composition", () =>
    Throws<InvalidOperationException>(() => new ModuleCatalog([
        Manifest("alpha", new ModuleDependency("beta", new(1, 0, 0))), Manifest("beta", new ModuleDependency("alpha", new(1, 0, 0)))])));
Test("M1-S1-18 duplicate and reserved IDs block composition", () =>
{
    Throws<InvalidOperationException>(() => new ModuleCatalog([Manifest("alpha"), Manifest("alpha")]));
    Throws<InvalidOperationException>(() => new ModuleCatalog([Manifest("core")]));
});
Test("M1-S1-19 invalid manifest version, IDs, and cross-owner permission denied", () =>
{
    Throws<ArgumentException>(() => new ModuleManifest("alpha", new(0, 1, 0), "Tests", [], []));
    Throws<ArgumentException>(() => new ModuleManifest("alpha", new(1, 0), "Tests", [], []));
    Throws<ArgumentException>(() => Manifest("../alpha"));
    Throws<ArgumentException>(() => new ModuleManifest("alpha", new(1, 0, 0), "Tests", [], [new("alpha.read", "beta.read")]));
});
Test("M1-S1-20 activation cannot bypass disabled dependency", () =>
{
    var dependencyCatalog = new ModuleCatalog([Manifest("alpha", new ModuleDependency("beta", new(1, 0, 0))), Manifest("beta")]);
    Require(!dependencyCatalog.IsOperational("alpha", Set("alpha", "beta"), Set("alpha")));
    Require(dependencyCatalog.IsOperational("alpha", Set("alpha", "beta"), Set("alpha", "beta")));
});
Test("M1-S1-21 manifest freezes caller-owned collections", () =>
{
    var dependencies = new[] { new ModuleDependency("core", new(1, 0, 0)) };
    var manifest = Manifest("alpha", dependencies);
    dependencies[0] = new("missing", new(1, 0, 0));
    Require(new ModuleCatalog([manifest]).Modules["alpha"].Dependencies[0].Id == "core");
});
Test("M1-S1-22 core source and assembly dependency boundaries", () =>
{
    var root = new DirectoryInfo(AppContext.BaseDirectory);
    while (root is not null && !File.Exists(Path.Combine(root.FullName, "Wissian.slnx"))) root = root.Parent;
    Require(root is not null, "Executar a partir do checkout.");
    var corePath = Path.Combine(root!.FullName, "src", "Wissian.Core");
    var forbidden = new Regex(@"(?i)(clubal|base44|nexusdevhub|codexcheckpoint|\bTEM\b|\bOWM\b|\bGRA\b)");
    foreach (var file in Directory.EnumerateFiles(corePath, "*", SearchOption.AllDirectories)
        .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
            && !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)))
        Require(!forbidden.IsMatch(File.ReadAllText(file)), $"Identidade indevida em {Path.GetFileName(file)}");
    var project = XDocument.Load(Path.Combine(corePath, "Wissian.Core.csproj"));
    Require(!project.Descendants("ProjectReference").Any() && !project.Descendants("PackageReference").Any());
    Require(typeof(Organization).Assembly.GetReferencedAssemblies().All(a => a.Name!.StartsWith("System.", StringComparison.Ordinal)));
});

AsyncTest("M1-S1-23 HTTP neutral composition exposes no test module", async () =>
{
    await using var app = HostApplication.BuildNeutral(["--urls", "http://127.0.0.1:0", "--Logging:LogLevel:Default", "Warning"], []);
    await app.StartAsync();
    using var client = Client(app.Services);
    Require((await client.GetAsync("/health/live")).StatusCode == HttpStatusCode.OK);
    Require((await client.GetAsync("/api/foundation-probe")).StatusCode == HttpStatusCode.NotFound);
    Require((await client.GetAsync($"/api/organizations/{Guid.NewGuid()}/counterparties")).StatusCode == HttpStatusCode.NotFound);
    Require((await client.GetAsync("/")).StatusCode == HttpStatusCode.NotFound);
    Require(app.Services.GetService<Wissian.Counterparties.CounterpartiesDbContext>() is null);
    Require(!typeof(HostApplication).Assembly.GetReferencedAssemblies().Any(a => a.Name!.StartsWith("Wissian.Test", StringComparison.Ordinal)));
    await app.StopAsync();
});
AsyncTest("M1-S1-24 HTTP separately compiled extension consumes same core", async () =>
{
    await using var app = HostApplication.BuildNeutral(["--urls", "http://127.0.0.1:0", "--Logging:LogLevel:Default", "Warning"], [FoundationProbe.Registration]);
    await app.StartAsync();
    using var client = Client(app.Services);
    var result = await client.GetFromJsonAsync<Dictionary<string, string>>("/api/foundation-probe");
    Require(result?["coreAssembly"] == "Wissian.Core" && result["contractVersion"] == "1.0.0");
    Require(typeof(FoundationProbe).Assembly != typeof(Organization).Assembly);
    await app.StopAsync();
});
Test("M1-S1-25 invalid composition rejected before mapping endpoints", () =>
{
    var mapped = false;
    Throws<InvalidOperationException>(() => HostApplication.BuildNeutral([], [new(
        Manifest("alpha", new ModuleDependency("missing", new(1, 0, 0))), _ => mapped = true)]));
    Require(!mapped);
});

static HttpClient Client(IServiceProvider services)
{
    var addresses = services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!;
    return new HttpClient { BaseAddress = new Uri(addresses.Addresses.Single()), Timeout = TimeSpan.FromSeconds(10) };
}

var failures = 0;
foreach (var test in tests)
{
    try { await test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception error) { failures++; Console.Error.WriteLine($"FAIL {test.Name}: {error}"); }
}
Console.WriteLine($"Foundation: {tests.Count - failures}/{tests.Count} passed; {failures} failed.");
return failures == 0 ? 0 : 1;
