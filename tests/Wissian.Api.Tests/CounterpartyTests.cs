using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wissian.Core.Identity;
using Wissian.Core.Modularity;
using Wissian.Counterparties;
using Wissian.Host.Identity;
using static Wissian.Api.Tests.ApiLab;
using static Wissian.Api.Tests.LabClient;

namespace Wissian.Api.Tests;

internal static class CounterpartyTests
{
    internal static async Task EnableAsync(ApiLab lab, SyntheticPerson person, bool write = true)
    {
        await lab.ChangeAsync(async db =>
        {
            if (!await db.OrganizationModules.AnyAsync(m => m.OrganizationId == person.OrganizationId && m.ModuleId == "counterparties"))
                db.OrganizationModules.Add(new OrganizationModule(person.OrganizationId, "counterparties", true, true));
            db.Permissions.Add(new MembershipPermission(person.OrganizationId, person.MembershipId, "counterparties.read"));
            if (write) db.Permissions.Add(new MembershipPermission(person.OrganizationId, person.MembershipId, "counterparties.write"));
        });
    }
    public static async Task<int> RunAsync(ApiLab lab)
    {
        var tests = new List<(string Name, Func<Task> Run)>();
        void Test(string name, Func<Task> run) => tests.Add((name, run));
        async Task<(SyntheticPerson Person, LabClient Client, long Version)> Actor(bool write = true)
        {
            var person = await lab.PersonAsync(); await EnableAsync(lab, person, write);
            var client = lab.Client(); await client.LoginAsync(person); await client.SelectAsync(person);
            return (person, client, (await client.SessionAsync()).Version);
        }
        string Url(SyntheticPerson p, Guid? id = null) => $"/api/organizations/{p.OrganizationId}/counterparties" + (id is null ? "" : "/" + id);
        CounterpartyCommand Create(long v, string name = "Empresa Exemplo") => new(Guid.NewGuid(), v, null, "create", new(name, "organization", "customer", "REF-SINTETICA"));
        async Task<CounterpartyMutation> Apply(LabClient client, string url, CounterpartyCommand request)
        {
            using var response = await client.PostAsync(url, request); await Status(response, 200);
            return (await response.Content.ReadFromJsonAsync<CounterpartyMutation>())!;
        }
        Test("M2-S1-01 create, search, read, edit, archive and restore persist", async () =>
        {
            var (person, client, version) = await Actor(); using var owner = client;
            var created = await Apply(client, Url(person), Create(version, "  Cliente sintético  "));
            var item = await client.Http.GetFromJsonAsync<CounterpartyView>(Url(person, created.Id));
            Ensure(item is { Name: "Cliente sintético", Version: 1, IsArchived: false });
            var command = new CounterpartyCommand(Guid.NewGuid(), version, 1, "edit", new("Cliente atualizado", "person", "both", null));
            var updated = await Apply(client, Url(person, created.Id), command); Ensure(updated.Version == 2);
            await Apply(client, Url(person, created.Id), command with { CommandId = Guid.NewGuid(), Action = "archive", ExpectedVersion = 2, Value = null });
            Ensure((await client.Http.GetFromJsonAsync<CounterpartyPage>(Url(person)))!.Total == 0);
            Ensure((await client.Http.GetFromJsonAsync<CounterpartyPage>(Url(person) + "?status=archived&q=atualizado"))!.Total == 1);
            await Status(await client.PostAsync(Url(person, created.Id), command with { CommandId = Guid.NewGuid(), ExpectedVersion = 3 }), 409);
            await Apply(client, Url(person, created.Id), command with { CommandId = Guid.NewGuid(), Action = "restore", ExpectedVersion = 3, Value = null });
            await using var db = lab.CounterpartiesDb();
            Ensure((await db.Counterparties.SingleAsync(x => x.Id == created.Id)).Version == 4);
            Ensure(await db.Receipts.CountAsync(x => x.CounterpartyId == created.Id) == 4);
            await using var core = lab.OwnerDb();
            Ensure(await core.AuditEvents.CountAsync(x => x.ResourceId == created.Id.ToString() && x.ResourceType == "counterparty") == 4);
        });
        Test("M2-S1-02 anonymous, missing context, CSRF and foreign organization denied", async () =>
        {
            var (a, client, v) = await Actor(); using var owner = client;
            var b = await lab.PersonAsync(); await EnableAsync(lab, b);
            using var anonymous = lab.Client();
            await Status(await anonymous.Http.GetAsync(Url(a)), 401);
            using var noContext = lab.Client(); await noContext.LoginAsync(a);
            await Status(await noContext.Http.GetAsync(Url(a)), 404);
            await Status(await client.PostAsync(Url(a), Create(v), csrf: false), 400);
            var item = await Apply(client, Url(a), Create(v));
            await Status(await client.Http.GetAsync(Url(b)), 404);
            await Status(await client.Http.GetAsync(Url(b, item.Id)), 404);
            await Status(await client.PostAsync(Url(b), Create(v)), 404);
            using var other = lab.Client(); await other.LoginAsync(b); await other.SelectAsync(b);
            Ensure((await other.Http.GetFromJsonAsync<CounterpartyPage>(Url(b)))!.Total == 0);
            await Status(await other.Http.GetAsync(Url(b, item.Id)), 404);
        });
        Test("M2-S1-03 read permission never grants write", async () =>
        {
            var (person, client, v) = await Actor(write: false); using var owner = client;
            await Status(await client.Http.GetAsync(Url(person)), 200);
            await Status(await client.PostAsync(Url(person), Create(v)), 404);
            Ensure(!(await client.SessionAsync()).Context!.Capabilities.Contains("counterparties.write"));
        });
        Test("M2-S1-04 same command replays once and changed payload conflicts", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            var request = Create(v);
            var results = await Task.WhenAll(Apply(client, Url(person), request), Apply(client, Url(person), request));
            Ensure(results[0].Id == results[1].Id && results.Count(x => x.Replayed) == 1);
            await Status(await client.PostAsync(Url(person), request with { Value = request.Value! with { Name = "Mudança indevida" } }), 409);
            await using var db = lab.CounterpartiesDb();
            Ensure(await db.Counterparties.CountAsync(x => x.OrganizationId == person.OrganizationId) == 1);
            Ensure(await db.Receipts.CountAsync(x => x.OrganizationId == person.OrganizationId) == 1);
        });
        Test("M2-S1-05 concurrent versions have one winner and preserve loser", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            var item = await Apply(client, Url(person), Create(v));
            var request = new CounterpartyCommand(Guid.NewGuid(), v, 1, "edit", new("Versão A", "organization", "both", null));
            using var second = lab.Client(); await second.LoginAsync(person); await second.SelectAsync(person);
            var results = await Task.WhenAll(client.PostAsync(Url(person, item.Id), request),
                second.PostAsync(Url(person, item.Id), request with { CommandId = Guid.NewGuid(), Value = request.Value! with { Name = "Versão B" } }));
            Ensure(results.Count(x => (int)x.StatusCode == 200) == 1 && results.Count(x => (int)x.StatusCode == 409) == 1);
            foreach (var result in results) result.Dispose();
            Ensure((await client.Http.GetFromJsonAsync<CounterpartyView>(Url(person, item.Id)))!.Version == 2);
        });
        Test("M2-S1-06 revocation blocks reads, commands and prior receipts", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            var request = Create(v); var item = await Apply(client, Url(person), request);
            await lab.ChangeAsync(async db => (await db.Permissions.SingleAsync(p => p.MembershipId == person.MembershipId
                && p.Permission == "counterparties.write")).Revoke());
            await Status(await client.PostAsync(Url(person), request), 404);
            await Status(await client.Http.GetAsync(Url(person, item.Id)), 200);
            await lab.ChangeAsync(async db => (await db.OrganizationModules.SingleAsync(p => p.OrganizationId == person.OrganizationId
                && p.ModuleId == "counterparties")).Suspend());
            await Status(await client.Http.GetAsync(Url(person, item.Id)), 404);
            Ensure(!(await client.SessionAsync()).Context!.Capabilities.Contains("counterparties.read"));
        });
        Test("M2-S1-07 invalid shape, bounds, action and stale context have no effect", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            foreach (var input in new[] { new CounterpartyInput(" ", "organization", "customer", null), new(new('x',161), "person", "customer", null),
                new("A\u0001B", "person", "customer", null), new("Nome", "unknown", "customer", null), new("Nome", "person", "unknown", null),
                new("Nome", "person", "supplier", new('x',81)) })
                await Status(await client.PostAsync(Url(person), Create(v) with { Value = input }), 400);
            await Status(await client.PostAsync(Url(person), new { commandId = Guid.NewGuid(), expectedSessionVersion = v, action = "create", organizationId = Guid.NewGuid() }), 400);
            await Status(await client.PostAsync(Url(person), Create(v) with { ExpectedSessionVersion = v - 1 }), 409);
            await Status(await client.Http.GetAsync(Url(person) + "?page=0"), 400);
            await Status(await client.Http.GetAsync(Url(person) + "?status=unknown"), 400);
            await using var db = lab.CounterpartiesDb(); Ensure(!await db.Counterparties.AnyAsync(x => x.OrganizationId == person.OrganizationId));
        });
        Test("M2-S1-08 RLS hides rows without session and with forged organization", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            await Apply(client, Url(person), Create(v));
            await using var db = lab.CounterpartiesDb(runtime: true);
            Ensure(await db.Counterparties.CountAsync() == 0);
            await using var tx = await db.Database.BeginTransactionAsync();
            var session = await client.SessionAsync(); var actor = person.UserId.ToString(); var org = person.OrganizationId.ToString(); var sid = session.SessionId.ToString();
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('wissian.actor_id',{actor},true), set_config('wissian.organization_id',{org},true), set_config('wissian.session_id',{sid},true)");
            Ensure(await db.Counterparties.CountAsync() == 1);
            var foreign = Guid.NewGuid().ToString();
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('wissian.organization_id',{foreign},true)");
            Ensure(await db.Counterparties.CountAsync() == 0);
        });
        Test("M2-S1-09 runtime cannot delete, move ownership or rewrite receipt", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            var item = await Apply(client, Url(person), Create(v));
            foreach (var sql in new[] { "DELETE FROM wissian_counterparties.counterparties", "UPDATE wissian_counterparties.counterparties SET \"OrganizationId\" = gen_random_uuid()",
                "UPDATE wissian_counterparties.command_receipts SET \"RequestHash\" = repeat('0',64)" })
            {
                await using var db = lab.CounterpartiesDb(runtime: true);
                try { await db.Database.ExecuteSqlRawAsync(sql); throw new InvalidOperationException("Privilégio indevido."); }
                catch (PostgresException e) when (e.SqlState == "42501") { }
            }
            Ensure((await client.Http.GetFromJsonAsync<CounterpartyView>(Url(person, item.Id)))!.Version == 1);
        });
        Test("M2-S1-10 revocation committed while request waits wins before mutation", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            await using var db = lab.OwnerDb(); await using var tx = await db.Database.BeginTransactionAsync();
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE wissian.membership_permissions SET \"IsActive\"=false WHERE \"MembershipId\"={person.MembershipId} AND \"Permission\"='counterparties.write'");
            var pending = client.PostAsync(Url(person), Create(v));
            await using var admin = new NpgsqlConnection(Environment.GetEnvironmentVariable("WISSIAN_TEST_ADMIN_CONNECTION"));
            await admin.OpenAsync(); var observed = false;
            for (var i = 0; i < 60; i++)
            {
                await using var query = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM pg_stat_activity WHERE datname='wissian_api_synthetic' AND usename='wissian_runtime' AND wait_event_type='Lock')", admin);
                if ((bool)(await query.ExecuteScalarAsync())!) { observed = true; break; }
                await Task.Delay(25);
            }
            Ensure(observed && !pending.IsCompleted, "Pedido não foi observado esperando a autoridade.");
            await tx.CommitAsync(); await Status(await pending, 404);
            await using var module = lab.CounterpartiesDb(); Ensure(!await module.Counterparties.AnyAsync(x => x.OrganizationId == person.OrganizationId));
        });
        Test("M2-S1-11 failed audit rolls back entity and idempotency receipt", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            await using var db = lab.OwnerDb();
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE wissian.audit_events ADD CONSTRAINT synthetic_reject_counterparty CHECK (\"Action\" <> 'counterparties.create') NOT VALID");
            try { await Status(await client.PostAsync(Url(person), Create(v)), 500); }
            finally { await db.Database.ExecuteSqlRawAsync("ALTER TABLE wissian.audit_events DROP CONSTRAINT synthetic_reject_counterparty"); }
            await using var module = lab.CounterpartiesDb();
            Ensure(!await module.Counterparties.AnyAsync(x => x.OrganizationId == person.OrganizationId)
                && !await module.Receipts.AnyAsync(x => x.OrganizationId == person.OrganizationId));
        });
        Test("M2-S1-12 pagination and literal filters never reveal another tenant", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            await using (var db = lab.CounterpartiesDb())
            {
                for (var i=0;i<27;i++) db.Counterparties.Add(new(Guid.NewGuid(), person.OrganizationId,
                    new($"Nome {i:00}", "organization", "supplier", null), lab.Clock.GetUtcNow()));
                await db.SaveChangesAsync();
            }
            var first = (await client.Http.GetFromJsonAsync<CounterpartyPage>(Url(person)))!;
            var second = (await client.Http.GetFromJsonAsync<CounterpartyPage>(Url(person) + "?page=2"))!;
            Ensure(first.Total == 27 && first.Items.Count == 25 && second.Items.Count == 2 && !first.Items.Select(x=>x.Id).Intersect(second.Items.Select(x=>x.Id)).Any());
            Ensure((await client.Http.GetFromJsonAsync<CounterpartyPage>(Url(person)+"?q=%25"))!.Total == 0);
            Ensure((await client.Http.GetFromJsonAsync<CounterpartyPage>(Url(person)+"?q=nome%2001"))!.Total == 1);
        });
        Test("M2-S1-13 restart reads persisted records and repeated migration is inert", async () =>
        {
            var (person, client, v) = await Actor(); using var owner = client;
            var item = await Apply(client, Url(person), Create(v));
            await using (var db = lab.CounterpartiesDb()) await db.Database.MigrateAsync();
            await using var restarted = lab.Build(); await restarted.StartAsync();
            var address = restarted.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
                .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()!.Addresses.Single();
            using var fresh = lab.Client(new(address)); await fresh.LoginAsync(person); await fresh.SelectAsync(person);
            Ensure((await fresh.Http.GetFromJsonAsync<CounterpartyView>(Url(person,item.Id)))!.Name == "Empresa Exemplo");
            await restarted.StopAsync();
        });
        Test("M2-S1-14 foreign organization cannot acquire authority locks", async () =>
        {
            var (person, client, _) = await Actor(); using var owner = client;
            var foreign = await lab.PersonAsync(); await EnableAsync(lab, foreign);
            await using var db = lab.OwnerDb(); await using var tx = await db.Database.BeginTransactionAsync();
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE wissian.organizations SET \"DisplayName\"=\"DisplayName\" WHERE \"Id\"={foreign.OrganizationId}");
            // The foreign row is locked until disposal. An unauthorized request must not wait for it.
            await Status(await client.Http.GetAsync(Url(foreign)).WaitAsync(TimeSpan.FromSeconds(2)), 404);
            Ensure((await client.SessionAsync()).Context!.OrganizationId == person.OrganizationId);
        });
        var failures = 0;
        foreach (var test in tests)
        {
            try { await test.Run(); Console.WriteLine("PASS " + test.Name); }
            catch (Exception e) { failures++; Console.Error.WriteLine("FAIL " + test.Name + ": " + e.Message); }
        }
        Console.WriteLine($"Counterparties: {tests.Count - failures}/{tests.Count} PASS; {failures} FAIL");
        return failures == 0 ? 0 : 1;
    }
}
