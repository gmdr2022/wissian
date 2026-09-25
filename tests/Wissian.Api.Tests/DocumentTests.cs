using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wissian.Core.Identity;
using Wissian.Core.Modularity;
using Wissian.Documents;
using Wissian.Host.Identity;
using Wissian.Host.Modules;
using static Wissian.Api.Tests.ApiLab;
using static Wissian.Api.Tests.LabClient;

namespace Wissian.Api.Tests;

// Compiled test composition only. Production has no configuration switch that accepts a fake scanner.
public sealed class TestAttachmentInspector : IAttachmentInspector
{
    public Func<Task>? BeforeScan { get; set; }
    public AttachmentInspection Result { get; set; } = new("accepted", "synthetic-fixture");
    public async Task<AttachmentInspection> InspectAsync(byte[] bytes, string extension, CancellationToken ct)
    {
        if (BeforeScan is not null) await BeforeScan();
        return Result;
    }
}

public static class DocumentTests
{
    public static async Task EnableAsync(ApiLab lab, SyntheticPerson person, bool write = true, bool restricted = true)
    {
        await lab.ChangeAsync(db =>
        {
            db.OrganizationModules.Add(new OrganizationModule(person.OrganizationId, "documents", true, true));
            db.Permissions.Add(new MembershipPermission(person.OrganizationId, person.MembershipId, "documents.read"));
            if (write) db.Permissions.Add(new MembershipPermission(person.OrganizationId, person.MembershipId, "documents.write"));
            if (restricted) db.Permissions.Add(new MembershipPermission(person.OrganizationId, person.MembershipId, "documents.restricted"));
            return Task.CompletedTask;
        });
    }
    public static async Task<int> RunAsync(ApiLab lab)
    {
        var tests = new List<(string Name, Func<Task> Run)>();
        void Test(string name, Func<Task> run) => tests.Add((name, run));
        async Task<(SyntheticPerson Person, LabClient Client, long Version)> Actor(bool write = true, bool restricted = true)
        {
            var person = await lab.PersonAsync(); await EnableAsync(lab, person, write, restricted);
            var client = lab.Client(); await client.LoginAsync(person); await client.SelectAsync(person);
            return (person, client, (await client.SessionAsync()).Version);
        }
        string Url(SyntheticPerson person) => $"/api/organizations/{person.OrganizationId}/documents";
        DocumentCommand Create(long v, string classification = "internal") => new(Guid.NewGuid(), v, null, null,
            "create", new("Documento sintético", classification, "Produzido para teste"));
        byte[] Bytes(string text = "Original sintético preservado.\n") => Encoding.UTF8.GetBytes(text);
        async Task<HttpResponseMessage> Upload(LabClient client, string url, DocumentCommand command, byte[]? bytes = null,
            string fileName = "original.txt", string mime = "text/plain", bool csrf = true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url + "/uploads");
            using var body = new MultipartFormDataContent();
            body.Add(JsonContent.Create(command), "command");
            var file = new ByteArrayContent(bytes ?? Bytes());
            file.Headers.ContentType = new(mime); body.Add(file, "file", fileName);
            request.Content = body;
            if (csrf && client.Csrf is not null) request.Headers.Add(IdentitySetup.CsrfHeader, client.Csrf);
            return await client.Http.SendAsync(request);
        }
        async Task<DocumentMutation> Mutation(HttpResponseMessage response)
        {
            using (response) { await Status(response, 200); return (await response.Content.ReadFromJsonAsync<DocumentMutation>())!; }
        }
        Test("M2-S2A-01 original, versions, metadata, archive and restore persist", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c; var url = Url(p);
            var request = Create(v); var first = await Mutation(await Upload(c, url, request));
            Ensure(first is { Version: 1, Revision: 1, ScanState: "accepted" });
            var next = request with { CommandId = Guid.NewGuid(), DocumentId = first.Id, ExpectedVersion = 1, Action = "revision", Value = null };
            var second = await Mutation(await Upload(c, url, next, Bytes("Segunda versão")));
            Ensure(second.Version == 2 && second.Revision == 2);
            foreach (var number in new[] { 1, 2 })
            {
                using var file = await c.Http.GetAsync(url + $"/{first.Id}/revisions/{number}/download");
                await Status(file, 200);
                Ensure((await file.Content.ReadAsByteArrayAsync()).SequenceEqual(number == 1 ? Bytes() : Bytes("Segunda versão")));
                Ensure(file.Content.Headers.ContentDisposition?.DispositionType == "attachment"
                    && file.Headers.CacheControl?.NoStore == true && file.Headers.GetValues("X-Content-Type-Options").Single() == "nosniff");
            }
            var edit = next with { CommandId = Guid.NewGuid(), ExpectedVersion = 2, Action = "edit", Value = new("Título atualizado", "internal", null) };
            await Mutation(await c.PostAsync(url + "/commands", edit));
            var archive = edit with { CommandId = Guid.NewGuid(), ExpectedVersion = 3, Action = "archive", Value = null };
            await Mutation(await c.PostAsync(url + "/commands", archive));
            Ensure((await c.Http.GetFromJsonAsync<DocumentPage>(url))!.Total == 0);
            Ensure((await c.Http.GetFromJsonAsync<DocumentPage>(url + "?status=archived&q=atualizado"))!.Total == 1);
            await Status(await Upload(c, url, next with { CommandId = Guid.NewGuid(), ExpectedVersion = 4 }), 409);
            await Mutation(await c.PostAsync(url + "/commands", archive with { CommandId = Guid.NewGuid(), Action = "restore", ExpectedVersion = 4 }));
            var detail = (await c.Http.GetFromJsonAsync<DocumentDetail>(url + "/" + first.Id))!;
            Ensure(detail.Document.Version == 5 && detail.Revisions.Count == 2 && detail.Revisions.Last().Sha256 == Convert.ToHexString(SHA256.HashData(Bytes())));
            await using var db = lab.DocumentsDb(); Ensure(await db.Originals.CountAsync(x => x.DocumentId == first.Id) == 2);
        });
        Test("M2-S2A-02 restricted data disappears from counts, details, versions and download", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c; var url = Url(p);
            var restricted = await Mutation(await Upload(c, url, Create(v, "restricted")));
            await Mutation(await Upload(c, url, Create(v)));
            await lab.ChangeAsync(async db => (await db.Permissions.SingleAsync(x => x.MembershipId == p.MembershipId && x.Permission == "documents.restricted")).Revoke());
            Ensure((await c.Http.GetFromJsonAsync<DocumentPage>(url + "?q=sintético"))!.Total == 1);
            await Status(await c.Http.GetAsync(url + "/" + restricted.Id), 404);
            await Status(await c.Http.GetAsync(url + $"/{restricted.Id}/revisions/1/download"), 404);
            await Status(await Upload(c, url, Create(v, "restricted")), 404);
            await using var db = lab.DocumentsDb(); Ensure(await db.Documents.CountAsync(x => x.OrganizationId == p.OrganizationId) == 2);
        });
        Test("M2-S2A-03 quarantine blocks downloads until new inspection and expires in 24h", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c; var url = Url(p);
            lab.Inspector.Result = AttachmentInspection.Unavailable;
            var created = await Mutation(await Upload(c, url, Create(v)));
            Ensure(created.ScanState == "quarantined");
            var path = url + $"/{created.Id}/revisions/1/download";
            await Status(await c.Http.GetAsync(path), 409);
            lab.Inspector.Result = new("accepted", "synthetic-fixture");
            var scan = new DocumentCommand(Guid.NewGuid(), v, created.Id, 1, "scan", null, 1);
            var result = await Mutation(await c.PostAsync(url + "/commands", scan));
            Ensure(result.ScanState == "accepted" && result.Version == 2);
            await Status(await c.Http.GetAsync(path), 200);
            await Mutation(await c.PostAsync(url + "/commands", scan with { CommandId = Guid.NewGuid(), Action = "archive", ExpectedVersion = 2, Revision = null }));
            await using (var db = lab.DocumentsDb())
                await db.Revisions.Where(x => x.DocumentId == created.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.ScannedAtUtc, DateTimeOffset.UtcNow.AddHours(-25)));
            await Status(await c.Http.GetAsync(path), 409);
            lab.Inspector.Result = new("blocked", "synthetic-fixture");
            await Mutation(await c.PostAsync(url + "/commands", scan with { CommandId = Guid.NewGuid(), ExpectedVersion = 3 }));
            await Status(await c.Http.GetAsync(path), 409);
            Ensure((await c.Http.GetFromJsonAsync<DocumentDetail>(url + "/" + created.Id))!.Document.IsArchived);
            await using var owner = lab.DocumentsDb(); Ensure(await owner.Originals.CountAsync(x => x.DocumentId == created.Id) == 1);
        });
        Test("M2-S2A-04 blocked new content creates no original, receipt or document", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            lab.Inspector.Result = new("blocked", "synthetic-fixture");
            await Status(await Upload(c, Url(p), Create(v)), 422);
            await using var db = lab.DocumentsDb();
            Ensure(!await db.Documents.AnyAsync(x => x.OrganizationId == p.OrganizationId)
                && !await db.Originals.AnyAsync(x => x.OrganizationId == p.OrganizationId) && !await db.Receipts.AnyAsync(x => x.OrganizationId == p.OrganizationId));
        });
        Test("M2-S2A-05 bounded allowlist rejects paths, mismatches, invalid UTF8 and archive", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            foreach (var (name, mime, bytes) in new (string, string, byte[])[] {
                ("../file.txt", "text/plain", Bytes()), ("file.exe", "text/plain", Bytes()),
                ("file.txt", "application/pdf", Bytes()), ("file.pdf", "application/pdf", Bytes()),
                ("file.zip", "application/zip", [80,75,3,4]), ("file.txt", "text/plain", [0xff,0xff]),
                ("file.txt", "text/plain", []), ("file.txt", "text/plain", [0]), ("file.png", "image/png", Bytes()) })
                await Status(await Upload(c, Url(p), Create(v), bytes, name, mime), 400);
            await Status(await Upload(c, Url(p), Create(v), new byte[AttachmentPolicy.MaxFileBytes + 16385]), 413);
            await using var db = lab.DocumentsDb(); Ensure(!await db.Documents.AnyAsync(x => x.OrganizationId == p.OrganizationId));
        });
        Test("M2-S2A-06 replay acknowledges one immutable effect and changed bytes conflict", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c; var request = Create(v);
            var replies = await Task.WhenAll(Upload(c, Url(p), request), Upload(c, Url(p), request));
            var a = await Mutation(replies[0]); var b = await Mutation(replies[1]);
            Ensure(a.Id == b.Id && a.Replayed != b.Replayed);
            await Status(await Upload(c, Url(p), request, Bytes("Mudança indevida")), 409);
            await using var db = lab.DocumentsDb(); Ensure(await db.Originals.CountAsync(x => x.DocumentId == a.Id) == 1);
            await using var core = lab.OwnerDb(); Ensure(await core.AuditEvents.CountAsync(x => x.ResourceId == a.Id.ToString() && x.Action == "documents.create") == 1);
        });
        Test("M2-S2A-07 anonymous, missing context, CSRF and foreign organization denied", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c; var other = await lab.PersonAsync(); await EnableAsync(lab, other);
            var item = await Mutation(await Upload(c, Url(p), Create(v)));
            using var anonymous = lab.Client(); await Status(await anonymous.Http.GetAsync(Url(p)), 401);
            using var noContext = lab.Client(); await noContext.LoginAsync(p); await Status(await noContext.Http.GetAsync(Url(p)), 404);
            await Status(await Upload(c, Url(p), Create(v), csrf: false), 400);
            await Status(await Upload(c, Url(other), Create(v)), 404);
            await Status(await c.Http.GetAsync(Url(other)), 404);
            using var foreign = lab.Client(); await foreign.LoginAsync(other); await foreign.SelectAsync(other);
            await Status(await foreign.Http.GetAsync(Url(other) + $"/{item.Id}/revisions/1/download"), 404);
            Ensure((await foreign.Http.GetFromJsonAsync<DocumentPage>(Url(other)))!.Total == 0);
        });
        Test("M2-S2A-08 concurrent versions, stale sessions and invalid commands preserve data", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c; var url = Url(p);
            var item = await Mutation(await Upload(c, url, Create(v)));
            var cmd = new DocumentCommand(Guid.NewGuid(), v, item.Id, 1, "edit", new("Atualização", "internal", null));
            var replies = await Task.WhenAll(c.PostAsync(url + "/commands", cmd), c.PostAsync(url + "/commands", cmd with { CommandId = Guid.NewGuid() }));
            Ensure(replies.Select(x => (int)x.StatusCode).Order().SequenceEqual([200,409])); foreach (var r in replies) r.Dispose();
            await Status(await Upload(c, url, Create(v - 1)), 409);
            await Status(await c.PostAsync(url + "/commands", cmd with { Action = "create" }), 400);
            await Status(await c.PostAsync(url + "/commands", new { commandId = Guid.NewGuid(), organizationId = p.OrganizationId }), 400);
            await Status(await Upload(c, url, Create(v) with { Value = new(" ", "internal", null) }), 400);
            await Status(await c.Http.GetAsync(url + "?page=0"), 400);
            await Status(await c.Http.GetAsync(url + "?status=unknown"), 400);
        });
        Test("M2-S2A-09 read never grants upload, revocation blocks existing links and receipts", async () =>
        {
            var (p, c, v) = await Actor(write: false); using var owned = c;
            await Status(await c.Http.GetAsync(Url(p)), 200); await Status(await Upload(c, Url(p), Create(v)), 404);
            var (writer, active, version) = await Actor(); using var activeOwned = active; var request = Create(version);
            var item = await Mutation(await Upload(active, Url(writer), request));
            await lab.ChangeAsync(async db => (await db.OrganizationModules.SingleAsync(x => x.OrganizationId == writer.OrganizationId && x.ModuleId == "documents")).Suspend());
            await Status(await active.Http.GetAsync(Url(writer) + $"/{item.Id}/revisions/1/download"), 404);
            await Status(await Upload(active, Url(writer), request), 404);
        });
        Test("M2-S2A-10 database RLS scopes original bytes as well as metadata", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            await Mutation(await Upload(c, Url(p), Create(v)));
            await using var db = lab.DocumentsDb(runtime: true);
            Ensure(await db.Documents.CountAsync() == 0 && await db.Originals.CountAsync() == 0);
            await using var tx = await db.Database.BeginTransactionAsync();
            var sid = (await c.SessionAsync()).SessionId.ToString(); var actor = p.UserId.ToString(); var org = p.OrganizationId.ToString();
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('wissian.actor_id',{actor},true), set_config('wissian.organization_id',{org},true), set_config('wissian.session_id',{sid},true)");
            Ensure(await db.Documents.CountAsync() == 1 && await db.Originals.CountAsync() == 1);
            var foreign = Guid.NewGuid().ToString(); await db.Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('wissian.organization_id',{foreign},true)");
            Ensure(await db.Documents.CountAsync() == 0 && await db.Originals.CountAsync() == 0 && await db.Revisions.CountAsync() == 0);
        });
        Test("M2-S2A-11 runtime cannot delete originals, rewrite bytes or move ownership", async () =>
        {
            foreach (var sql in new[] { "DELETE FROM wissian_documents.originals", "UPDATE wissian_documents.originals SET \"Bytes\" = '\\x00'",
                "UPDATE wissian_documents.revisions SET \"Sha256\" = 'x'", "UPDATE wissian_documents.documents SET \"OrganizationId\" = gen_random_uuid()",
                "UPDATE wissian_documents.command_receipts SET \"AppliedVersion\" = 999" })
            {
                await using var db = lab.DocumentsDb(runtime: true);
                try { await db.Database.ExecuteSqlRawAsync(sql); throw new InvalidOperationException("SQL privilegiado aceito"); }
                catch (PostgresException e) when (e.SqlState == "42501") { }
            }
        });
        Test("M2-S2A-12 audit failure rolls back metadata, bytes and receipt together", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            await using var core = lab.OwnerDb();
            await core.Database.ExecuteSqlRawAsync("ALTER TABLE wissian.audit_events ADD CONSTRAINT synthetic_documents_audit_failure CHECK (\"Action\" <> 'documents.create') NOT VALID");
            try { await Status(await Upload(c, Url(p), Create(v)), 500); }
            finally { await core.Database.ExecuteSqlRawAsync("ALTER TABLE wissian.audit_events DROP CONSTRAINT synthetic_documents_audit_failure"); }
            await using var db = lab.DocumentsDb(); Ensure(!await db.Documents.AnyAsync(x => x.OrganizationId == p.OrganizationId)
                && !await db.Originals.AnyAsync(x => x.OrganizationId == p.OrganizationId) && !await db.Receipts.AnyAsync(x => x.OrganizationId == p.OrganizationId));
        });
        Test("M2-S2A-13 revocation during scanner wait wins without database locks", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            lab.Inspector.BeforeScan = async () => { entered.SetResult(); await release.Task; };
            var upload = Upload(c, Url(p), Create(v));
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
                await lab.ChangeAsync(async db => (await db.Permissions.SingleAsync(x => x.MembershipId == p.MembershipId && x.Permission == "documents.write")).Revoke())
                    .WaitAsync(TimeSpan.FromSeconds(3));
            }
            finally { release.TrySetResult(); }
            await Status(await upload, 404);
            await using var db = lab.DocumentsDb(); Ensure(!await db.Originals.AnyAsync(x => x.OrganizationId == p.OrganizationId));
        });
        Test("M2-S2A-14 quota includes all classifications and archived originals", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            var bytes = Enumerable.Repeat((byte)'x', AttachmentPolicy.MaxFileBytes).ToArray();
            var first = await Mutation(await Upload(c, Url(p), Create(v, "restricted"), bytes));
            for (var i = 2; i <= 10; i++)
                await Mutation(await Upload(c, Url(p), new(Guid.NewGuid(), v, first.Id, i - 1, "revision", null), bytes));
            await Mutation(await c.PostAsync(Url(p) + "/commands", new DocumentCommand(Guid.NewGuid(), v, first.Id, 10, "archive", null)));
            await lab.ChangeAsync(async db => (await db.Permissions.SingleAsync(x => x.MembershipId == p.MembershipId && x.Permission == "documents.restricted")).Revoke());
            await Status(await Upload(c, Url(p), Create(v)), 409);
            await using var db = lab.DocumentsDb(); Ensure(await db.Documents.CountAsync(x => x.OrganizationId == p.OrganizationId) == 1
                && await db.Originals.CountAsync(x => x.OrganizationId == p.OrganizationId) == 10);
        });
        Test("M2-S2A-15 malformed multipart cannot persist partial originals", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            using var request = new HttpRequestMessage(HttpMethod.Post, Url(p) + "/uploads");
            request.Headers.Add(IdentitySetup.CsrfHeader, c.Csrf);
            request.Content = new StringContent("--boundary\r\nContent-Disposition: form-data; name=\"command\"\r\n\r\n{}", Encoding.UTF8);
            request.Content.Headers.ContentType = new("multipart/form-data"); request.Content.Headers.ContentType.Parameters.Add(new("boundary", "boundary"));
            await Status(await c.Http.SendAsync(request), 400);
            await using var db = lab.DocumentsDb(); Ensure(!await db.Originals.AnyAsync(x => x.OrganizationId == p.OrganizationId));
        });
        Test("M2-S2A-16 new host and repeated migration retain originals without counterparties", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            var item = await Mutation(await Upload(c, Url(p), Create(v)));
            await using (var db = lab.DocumentsDb()) { await db.Database.MigrateAsync(); Ensure(!(await db.Database.GetPendingMigrationsAsync()).Any()); }
            await using var app = lab.Build(); await app.StartAsync();
            try
            {
                var address = new Uri(app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
                    .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()!.Addresses.Single());
                using var client = lab.Client(address); await client.LoginAsync(p); await client.SelectAsync(p);
                using var file = await client.Http.GetAsync(Url(p) + $"/{item.Id}/revisions/1/download"); await Status(file, 200);
                Ensure((await file.Content.ReadAsByteArrayAsync()).SequenceEqual(Bytes()));
                Ensure(!app.Services.GetRequiredService<ModuleCatalog>().Modules.ContainsKey("counterparties"));
            }
            finally { await app.StopAsync(); }
        });
        Test("M2-S2A-17 inspector treats errors, unknown risk and policy blocks as unavailable", () =>
        {
            Ensure(WindowsAttachmentInspector.Classify(-1, 0).State == "quarantined");
            Ensure(WindowsAttachmentInspector.Classify(0, 0).State == "accepted" && WindowsAttachmentInspector.Classify(0, 1).State == "accepted");
            Ensure(WindowsAttachmentInspector.Classify(0, 2).State == "quarantined");
            foreach (uint risk in new uint[] { 0x4000, 0x4fff, 0x8000, 0xffff }) Ensure(WindowsAttachmentInspector.Classify(0, risk).State == "blocked");
            return Task.CompletedTask;
        });
        Test("M2-S2A-18 composite references and original hashes are database invariants", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            var item = await Mutation(await Upload(c, Url(p), Create(v)));
            await using (var wrong = lab.DocumentsDb())
            {
                wrong.Revisions.Add(new() { OrganizationId = Guid.NewGuid(), DocumentId = item.Id, Number = 2,
                    FileName = "original.txt", MediaType = "text/plain", Length = 1, Sha256 = new('a',64), Scanner = "fixture" });
                try { await wrong.SaveChangesAsync(); throw new InvalidOperationException("Referência composta aceita"); }
                catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23503" }) { }
            }
            await using (var owner = lab.DocumentsDb())
            {
                owner.Revisions.Add(new() { OrganizationId = p.OrganizationId, DocumentId = item.Id, Number = 2,
                    FileName = "original.txt", MediaType = "text/plain", Length = 1,
                    Sha256 = Convert.ToHexString(SHA256.HashData(new byte[] { 65 })), Scanner = "fixture" });
                await owner.SaveChangesAsync();
            }
            await using var runtime = lab.DocumentsDb(runtime: true);
            await using var tx = await runtime.Database.BeginTransactionAsync();
            var sid = (await c.SessionAsync()).SessionId.ToString(); var actor = p.UserId.ToString(); var org = p.OrganizationId.ToString();
            await runtime.Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('wissian.actor_id',{actor},true), set_config('wissian.organization_id',{org},true), set_config('wissian.session_id',{sid},true)");
            runtime.Originals.Add(new() { OrganizationId = p.OrganizationId, DocumentId = item.Id, Number = 2, Bytes = [66] });
            try { await runtime.SaveChangesAsync(); throw new InvalidOperationException("Hash divergente aceito"); }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { ConstraintName: "original_integrity" }) { }
        });
        Test("M2-S2A-19 concurrent sessions cannot consume the same remaining quota", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            var bytes = Enumerable.Repeat((byte)'q', AttachmentPolicy.MaxFileBytes).ToArray();
            var initial = await Mutation(await Upload(c, Url(p), Create(v), bytes));
            for (var i = 2; i <= 9; i++)
                await Mutation(await Upload(c, Url(p), new(Guid.NewGuid(), v, initial.Id, i - 1, "revision", null), bytes));
            using var second = lab.Client(); await second.LoginAsync(p); await second.SelectAsync(p);
            var replies = await Task.WhenAll(Upload(c, Url(p), Create(v), bytes),
                Upload(second, Url(p), Create((await second.SessionAsync()).Version), bytes));
            Ensure(replies.Select(r => (int)r.StatusCode).Order().SequenceEqual([200,409]));
            foreach (var response in replies) response.Dispose();
            await using var db = lab.DocumentsDb(); Ensure(await db.Originals.CountAsync(x => x.OrganizationId == p.OrganizationId) == 10);
        });
        Test("M2-S2A-20 slow inspections have two transfer slots and reject excess", async () =>
        {
            var (p, c, v) = await Actor(); using var owned = c;
            var count = 0;
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            lab.Inspector.BeforeScan = async () => { if (Interlocked.Increment(ref count) == 2) entered.SetResult(); await release.Task; };
            var first = Upload(c, Url(p), Create(v)); var second = Upload(c, Url(p), Create(v));
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
                await Status(await Upload(c, Url(p), Create(v)), 429);
            }
            finally { release.TrySetResult(); }
            await Mutation(await first); await Mutation(await second);
        });
        var failures = 0;
        var filter = Environment.GetEnvironmentVariable("WISSIAN_TEST_CASE");
        if (!string.IsNullOrEmpty(filter)) tests = tests.Where(t => t.Name.StartsWith(filter + " ", StringComparison.Ordinal)).ToList();
        if (tests.Count == 0) throw new InvalidOperationException("Caso não encontrado.");
        foreach (var test in tests)
        {
            try { await test.Run(); Console.WriteLine("PASS " + test.Name); }
            catch (Exception error) { failures++; Console.WriteLine($"FAIL {test.Name}: {error.GetType().Name}: {error.Message}; cause: {error.GetBaseException().Message}"); }
            finally { lab.Inspector.BeforeScan = null; lab.Inspector.Result = new("accepted", "synthetic-fixture"); }
        }
        Console.WriteLine($"DOCUMENTS: {tests.Count - failures}/{tests.Count} PASS; {failures} FAIL");
        return failures == 0 ? 0 : 1;
    }
}
