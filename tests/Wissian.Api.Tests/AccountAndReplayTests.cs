using System.Buffers.Binary;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wissian.Core.Identity;
using Wissian.Host.Identity;
using Wissian.Persistence;
using static Wissian.Api.Tests.ApiLab;
using static Wissian.Api.Tests.LabClient;

namespace Wissian.Api.Tests;

public static class AccountAndReplayTests
{
    public static void Register(Action<string, Func<Task>> test, ApiLab lab, Func<string, string> authenticatorCode)
    {
        test("M1-S3B-01 same command replays equivalent receipt with one effect", async () =>
        {
            var person = await lab.PersonAsync();
            using var client = lab.Client();
            await client.LoginAsync(person);
            var session = await client.SessionAsync();
            var payload = Selection(person, session.Version, Guid.NewGuid());
            using var first = await client.PostAsync("/api/identity/context", payload);
            using var replay = await client.PostAsync("/api/identity/context", payload);
            await Status(first, 200);
            await Status(replay, 200);
            Ensure(await first.Content.ReadAsStringAsync() == await replay.Content.ReadAsStringAsync());
            await using var db = lab.OwnerDb();
            Ensure(await db.ContextReceipts.CountAsync(r => r.SessionId == session.SessionId) == 1);
            Ensure(await db.AuditEvents.CountAsync(a => a.ActorId == person.UserId && a.Action == "identity.context_selected") == 1);
        });
        test("M1-S3B-02 concurrent replay returns one receipt twice", async () =>
        {
            var person = await lab.PersonAsync();
            using var client = lab.Client();
            await client.LoginAsync(person);
            var payload = Selection(person, (await client.SessionAsync()).Version, Guid.NewGuid());
            var replies = await Task.WhenAll(client.PostAsync("/api/identity/context", payload), client.PostAsync("/api/identity/context", payload));
            foreach (var reply in replies) await Status(reply, 200);
            Ensure(await replies[0].Content.ReadAsStringAsync() == await replies[1].Content.ReadAsStringAsync());
            foreach (var reply in replies) reply.Dispose();
            await using var db = lab.OwnerDb();
            Ensure(await db.ContextReceipts.CountAsync(r => r.UserId == person.UserId) == 1);
            Ensure(await db.AuditEvents.CountAsync(a => a.ActorId == person.UserId && a.Action == "identity.context_selected") == 1);
        });
        test("M1-S3B-03 reused key with different payload rolls back context and audit", async () =>
        {
            var person = await lab.PersonAsync();
            var another = await lab.PersonAsync();
            var member = Guid.NewGuid();
            await lab.ChangeAsync(db =>
            {
                db.Memberships.Add(new Membership(member, person.UserId, another.OrganizationId, lab.Clock.GetUtcNow()));
                return Task.CompletedTask;
            });
            using var client = lab.Client();
            await client.LoginAsync(person);
            var key = Guid.NewGuid();
            await Status(await client.PostAsync("/api/identity/context", Selection(person, 1, key)), 200);
            var previous = await client.SessionAsync();
            await Status(await client.PostAsync("/api/identity/context", Selection(person, previous.Version, key)), 409);
            await Status(await client.PostAsync("/api/identity/context", new ContextSelection(another.OrganizationId, member, previous.Version, key)), 409);
            var current = await client.SessionAsync();
            Ensure(current.Context!.OrganizationId == person.OrganizationId && current.Version == previous.Version);
            await using var db = lab.OwnerDb();
            Ensure(await db.AuditEvents.CountAsync(a => a.ActorId == person.UserId && a.Action == "identity.context_selected") == 1);
        });
        test("M1-S3B-04 receipt is denied after membership, organization or session revocation", async () =>
        {
            foreach (var mode in new[] { "membership", "organization", "session" })
            {
                var person = await lab.PersonAsync();
                using var client = lab.Client();
                await client.LoginAsync(person);
                var payload = Selection(person, 1, Guid.NewGuid());
                await Status(await client.PostAsync("/api/identity/context", payload), 200);
                var session = await client.SessionAsync();
                await lab.ChangeAsync(async db =>
                {
                    if (mode == "membership") (await db.Memberships.SingleAsync(m => m.Id == person.MembershipId)).Revoke();
                    if (mode == "organization") (await db.Organizations.SingleAsync(o => o.Id == person.OrganizationId)).Suspend();
                    if (mode == "session") (await db.Sessions.SingleAsync(s => s.Id == session.SessionId)).Revoke(lab.Clock.GetUtcNow());
                });
                using var response = await client.PostAsync("/api/identity/context", payload);
                await Status(response, mode == "session" ? 401 : 404);
                Ensure(!(await response.Content.ReadAsStringAsync()).Contains(session.SessionId.ToString(), StringComparison.Ordinal));
            }
        });
        test("M1-S3B-05 old receipt cannot restore context after a later selection", async () =>
        {
            var person = await lab.PersonAsync();
            var another = await lab.PersonAsync();
            var member = Guid.NewGuid();
            await lab.ChangeAsync(db => { db.Memberships.Add(new Membership(member, person.UserId, another.OrganizationId, lab.Clock.GetUtcNow())); return Task.CompletedTask; });
            using var client = lab.Client();
            await client.LoginAsync(person);
            var original = Selection(person, 1, Guid.NewGuid());
            await Status(await client.PostAsync("/api/identity/context", original), 200);
            await Status(await client.PostAsync("/api/identity/context", new ContextSelection(another.OrganizationId, member, 2, Guid.NewGuid())), 200);
            await Status(await client.PostAsync("/api/identity/context", original), 409);
            Ensure((await client.SessionAsync()).Context!.OrganizationId == another.OrganizationId);
        });
        test("M1-S3B-06 receipt RLS, append-only privilege and cross-user FK", async () =>
        {
            var person = await lab.PersonAsync();
            var other = await lab.PersonAsync();
            using var client = lab.Client();
            await client.LoginAsync(person);
            await client.SelectAsync(person);
            var session = await client.SessionAsync();
            foreach (var scope in new[] { (person.UserId, person.OrganizationId, 1), (other.UserId, person.OrganizationId, 0), (person.UserId, other.OrganizationId, 0) })
            {
                await using var db = RuntimeDb(lab);
                await using var transaction = await ScopedTransaction.BeginAsync(db, scope.Item1, scope.Item2);
                Ensure(await db.ContextReceipts.CountAsync(r => r.SessionId == session.SessionId) == scope.Item3);
            }
            await using (var db = RuntimeDb(lab))
            {
                await using var transaction = await ScopedTransaction.BeginAsync(db, person.UserId, person.OrganizationId);
                await Denied(() => db.Database.ExecuteSqlRawAsync("DELETE FROM wissian.context_receipts"), "42501");
            }
            await using (var db = lab.OwnerDb())
            {
                db.ContextReceipts.Add(new ContextSelectionReceipt(session.SessionId, Guid.NewGuid(), other.UserId,
                    other.OrganizationId, other.MembershipId, 1, new string('A', 64), session.Version, lab.Clock.GetUtcNow()));
                await Denied(() => db.SaveChangesAsync(), "23503");
            }
        });
        test("M1-S3B-07 recovery changes password and revokes every current session", async () =>
        {
            var person = await lab.PersonAsync();
            using var first = lab.Client();
            using var second = lab.Client();
            using var recovery = lab.Client();
            await first.LoginAsync(person);
            await second.LoginAsync(person);
            var link = await Issue(lab, person.UserId, "recovery");
            var password = NewPassword();
            await recovery.RefreshCsrfAsync();
            var request = new CompleteAccountAction(person.UserId, link.Token, password);
            await Status(await recovery.PostAsync("/api/identity/reset-password", request, csrf: false), 400);
            await Status(await recovery.PostAsync("/api/identity/reset-password", request), 200);
            await Status(await first.Http.GetAsync("/api/identity/session"), 401);
            await Status(await second.Http.GetAsync("/api/identity/session"), 401);
            await Status(await recovery.PostAsync("/api/identity/reset-password", request), 400);
            await recovery.LoginAsync(person with { Password = password });
            await using var db = lab.OwnerDb();
            Ensure(await db.Sessions.CountAsync(s => s.UserId == person.UserId && s.RevokedAtUtc != null) == 2);
            Ensure(await db.AuditEvents.CountAsync(a => a.ActorId == person.UserId && a.Action == "identity.password_reset") == 1);
        });
        test("M1-S3B-08 wrong user, purpose, expired and altered tokens are rejected", async () =>
        {
            var person = await lab.PersonAsync();
            var other = await lab.PersonAsync();
            var valid = await Issue(lab, person.UserId, "recovery");
            using var client = lab.Client();
            await client.RefreshCsrfAsync();
            await using var scope = lab.App.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
            var wrongPurpose = await users.GenerateUserTokenAsync((await users.FindByIdAsync(person.UserId.ToString()))!, TokenOptions.DefaultProvider, "unrelated-purpose");
            // Reissue only the timestamp using laboratory keys, preserving the actual provider format.
            var protector = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>().CreateProtector(AccountLinks.ProtectionName);
            var bytes = protector.Unprotect(Convert.FromBase64String(valid.Token));
            BinaryPrimitives.WriteInt64LittleEndian(bytes, DateTimeOffset.UtcNow.AddHours(-2).UtcTicks);
            var expired = Convert.ToBase64String(protector.Protect(bytes));
            foreach (var action in new[]
            {
                new CompleteAccountAction(other.UserId, valid.Token, NewPassword()),
                new(person.UserId, wrongPurpose, NewPassword()), new(person.UserId, expired, NewPassword()),
                new(person.UserId, "invalid" + valid.Token[7..], NewPassword()), new(Guid.NewGuid(), valid.Token, NewPassword())
            })
            {
                using var response = await client.PostAsync("/api/identity/reset-password", action);
                await Status(response, 400);
                Ensure((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString() == "access_link_invalid");
            }
            await client.LoginAsync(person);
        });
        test("M1-S3B-09 password validation failure leaves token usable without partial changes", async () =>
        {
            var person = await lab.PersonAsync();
            var link = await Issue(lab, person.UserId, "recovery");
            using var client = lab.Client();
            await client.RefreshCsrfAsync();
            await Status(await client.PostAsync("/api/identity/reset-password", new CompleteAccountAction(person.UserId, link.Token, "weak")), 400);
            await using (var db = lab.OwnerDb()) Ensure(!await db.AuditEvents.AnyAsync(a => a.ActorId == person.UserId && a.Action == "identity.password_reset"));
            await Status(await client.PostAsync("/api/identity/reset-password", new CompleteAccountAction(person.UserId, link.Token, NewPassword())), 200);
        });
        test("M1-S3B-10 concurrent token consumption has exactly one successful effect", async () =>
        {
            var person = await lab.PersonAsync();
            var link = await Issue(lab, person.UserId, "recovery");
            using var first = lab.Client();
            using var second = lab.Client();
            await first.RefreshCsrfAsync();
            await second.RefreshCsrfAsync();
            var request = new CompleteAccountAction(person.UserId, link.Token, NewPassword());
            var replies = await Task.WhenAll(first.PostAsync("/api/identity/reset-password", request), second.PostAsync("/api/identity/reset-password", request));
            Ensure(replies.Select(r => (int)r.StatusCode).Order().SequenceEqual([200, 400]));
            foreach (var response in replies) response.Dispose();
            await using var db = lab.OwnerDb();
            Ensure(await db.AuditEvents.CountAsync(a => a.ActorId == person.UserId && a.Action == "identity.password_reset") == 1);
        });
        test("M1-S3B-11 activation is one use and grants no organization membership", async () =>
        {
            var person = await lab.PersonAsync(membership: false, pending: true);
            var link = await Issue(lab, person.UserId, "activation");
            using var client = lab.Client();
            await client.RefreshCsrfAsync();
            var password = NewPassword();
            var request = new CompleteAccountAction(person.UserId, link.Token, password);
            await Status(await client.PostAsync("/api/identity/reset-password", request), 400);
            await Status(await client.PostAsync("/api/identity/activate", request), 200);
            await Status(await client.PostAsync("/api/identity/activate", request), 400);
            await client.LoginAsync(person with { Password = password });
            Ensure((await client.SessionAsync()).Organizations.Count == 0);
            await using var db = lab.OwnerDb();
            var user = await db.Users.SingleAsync(u => u.Id == person.UserId);
            Ensure(user.EmailConfirmed && user.IsActive && user.PasswordHash is not null);
        });
        test("M1-S3B-12 links cannot reactivate a suspended existing account", async () =>
        {
            var person = await lab.PersonAsync();
            var original = await Issue(lab, person.UserId, "recovery");
            await lab.ChangeAsync(async db => (await db.Users.SingleAsync(u => u.Id == person.UserId)).IsActive = false);
            await using var scope = lab.App.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
            Ensure(await AccountLinks.IssueAsync(users, person.UserId, "activation") is null);
            Ensure(await AccountLinks.IssueAsync(users, person.UserId, "recovery") is null);
            using var client = lab.Client();
            await client.RefreshCsrfAsync();
            await Status(await client.PostAsync("/api/identity/reset-password", new CompleteAccountAction(person.UserId, original.Token, NewPassword())), 400);
        });
        test("M1-S3B-13 access help is truthful and issuance has no public endpoint", async () =>
        {
            using var client = lab.Client();
            using var help = await client.Http.GetAsync("/api/identity/access-help");
            await Status(help, 200);
            Ensure((await help.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("mode").GetString() == "assisted");
            await Status(await client.PostAsync("/api/identity/issue", new { userId = Guid.NewGuid() }), 404);
        });
        test("M1-S3B-14 local operator creates private consumable activation file", async () =>
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            var person = await lab.PersonAsync(pending: true);
            await using var app = lab.Build(persistentKeys: true);
            await app.StartAsync();
            var result = await RunOperator(lab, person.UserId, "activation");
            Ensure(result.Exit == 0, "Operador falhou: " + result.Error);
            var path = result.Output.Trim();
            Ensure(File.Exists(path));
            var link = JsonSerializer.Deserialize<AccountActionLink>(await File.ReadAllTextAsync(path), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            Ensure(!result.Output.Contains(link.Token, StringComparison.Ordinal));
            using var identity = WindowsIdentity.GetCurrent();
            var rules = new FileInfo(path).GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>().ToArray();
            Ensure(rules.Length > 0);
            foreach (var rule in rules)
                Ensure(rule.IdentityReference.Equals(identity.User) && rule.AccessControlType == AccessControlType.Allow);
            using var client = lab.Client(new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()));
            await client.RefreshCsrfAsync();
            var password = NewPassword();
            await Status(await client.PostAsync("/api/identity/activate", new CompleteAccountAction(person.UserId, link.Token, password)), 200);
            await client.LoginAsync(person with { Password = password });
            await client.SelectAsync(person);
            await app.StopAsync();
            var refused = await RunOperator(lab, person.UserId, "recovery", invalidDatabase: true);
            Ensure(refused.Exit == 2 && string.IsNullOrWhiteSpace(refused.Output));
        });
        test("M1-S3B-15 audit failure rolls back password, stamp and session revocations", async () =>
        {
            var person = await lab.PersonAsync();
            using var signedIn = lab.Client();
            using var recovery = lab.Client();
            await signedIn.LoginAsync(person);
            await signedIn.SelectAsync(person);
            var link = await Issue(lab, person.UserId, "recovery");
            var request = new CompleteAccountAction(person.UserId, link.Token, NewPassword());
            await recovery.RefreshCsrfAsync();
            string? stamp;
            string? passwordHash;
            await using (var db = lab.OwnerDb())
            {
                var user = await db.Users.SingleAsync(u => u.Id == person.UserId);
                stamp = user.SecurityStamp;
                passwordHash = user.PasswordHash;
                await db.Database.ExecuteSqlRawAsync("REVOKE INSERT ON wissian.audit_events FROM wissian_runtime");
            }
            try { await Status(await recovery.PostAsync("/api/identity/reset-password", request), 500); }
            finally
            {
                await using var db = lab.OwnerDb();
                await db.Database.ExecuteSqlRawAsync("GRANT INSERT ON wissian.audit_events TO wissian_runtime");
            }
            await using (var db = lab.OwnerDb())
            {
                var user = await db.Users.SingleAsync(u => u.Id == person.UserId);
                Ensure(user.SecurityStamp == stamp && user.PasswordHash == passwordHash);
                Ensure(await db.Sessions.AllAsync(s => s.UserId != person.UserId || s.RevokedAtUtc == null));
                Ensure(!await db.AuditEvents.AnyAsync(a => a.ActorId == person.UserId && a.Action == "identity.password_reset"));
            }
            await Status(await signedIn.Http.GetAsync("/api/identity/session"), 200);
            await Status(await recovery.PostAsync("/api/identity/reset-password", request), 200);
        });
        test("M1-S3B-16 password recovery preserves MFA and revokes an unfinished challenge", async () =>
        {
            var person = await lab.PersonAsync();
            string key;
            await using (var scope = lab.App.Services.CreateAsyncScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
                var user = (await users.FindByIdAsync(person.UserId.ToString()))!;
                Ensure((await users.ResetAuthenticatorKeyAsync(user)).Succeeded);
                Ensure((await users.SetTwoFactorEnabledAsync(user, true)).Succeeded);
                key = (await users.GetAuthenticatorKeyAsync(user))!;
            }
            using var pending = lab.Client();
            using var recovery = lab.Client();
            await pending.RefreshCsrfAsync();
            await Status(await pending.PostAsync("/api/identity/login", new { identifier = person.Email, password = person.Password }), 202);
            var link = await Issue(lab, person.UserId, "recovery");
            var password = NewPassword();
            await recovery.RefreshCsrfAsync();
            await Status(await recovery.PostAsync("/api/identity/reset-password", new CompleteAccountAction(person.UserId, link.Token, password)), 200);
            await Status(await pending.PostAsync("/api/identity/mfa", new { code = authenticatorCode(key) }), 401);
            await Status(await recovery.PostAsync("/api/identity/login", new { identifier = person.Email, password }), 202);
            await Status(await recovery.Http.GetAsync("/api/identity/session"), 401);
            await Status(await recovery.PostAsync("/api/identity/mfa", new { code = authenticatorCode(key) }), 200);
            await using var db = lab.OwnerDb();
            Ensure((await db.Users.SingleAsync(u => u.Id == person.UserId)).TwoFactorEnabled);
        });
    }

    private static ContextSelection Selection(SyntheticPerson person, long version, Guid key) =>
        new(person.OrganizationId, person.MembershipId, version, key);
    private static string NewPassword() => "New1!" + Guid.NewGuid().ToString("N");
    private static async Task<AccountActionLink> Issue(ApiLab lab, Guid id, string purpose)
    {
        await using var scope = lab.App.Services.CreateAsyncScope();
        return await AccountLinks.IssueAsync(scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>(), id, purpose)
            ?? throw new InvalidOperationException("Fixture de link inválida.");
    }
    private static WissianDbContext RuntimeDb(ApiLab lab) => new(new DbContextOptionsBuilder<WissianDbContext>()
        .UseNpgsql(lab.Connection("wissian_runtime")).Options);
    private static async Task Denied(Func<Task> action, string state)
    {
        try { await action(); }
        catch (PostgresException error) when (error.SqlState == state) { return; }
        catch (DbUpdateException error) when (error.InnerException is PostgresException pg && pg.SqlState == state) { return; }
        throw new InvalidOperationException("Operação deveria ser negada: " + state);
    }
    private static async Task<(int Exit, string Output, string Error)> RunOperator(ApiLab lab, Guid userId, string purpose, bool invalidDatabase = false)
    {
        var dotnet = Path.Combine(Environment.GetEnvironmentVariable("DOTNET_ROOT")!, "dotnet.exe");
        var directory = Path.GetFullPath(Path.Combine(".local", "operator-tests", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        var start = new ProcessStartInfo(dotnet)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
            WorkingDirectory = directory
        };
        foreach (var argument in new[] { "exec", "--runtimeconfig", Path.Combine(AppContext.BaseDirectory, "Wissian.Api.Tests.runtimeconfig.json"),
            "--depsfile", Path.Combine(AppContext.BaseDirectory, "Wissian.Api.Tests.deps.json"),
            Path.Combine(AppContext.BaseDirectory, "Wissian.Operator.dll"), purpose, userId.ToString(), "--synthetic" }) start.ArgumentList.Add(argument);
        start.Environment["Wissian__Database"] = invalidDatabase
            ? new NpgsqlConnectionStringBuilder(lab.Connection("wissian_runtime")) { Database = "production" }.ConnectionString
            : lab.Connection("wissian_runtime");
        start.Environment["Wissian__KeyDirectory"] = lab.KeyDirectory;
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(entireProcessTree: true); throw new InvalidOperationException("Operador sintético excedeu prazo."); }
        return (process.ExitCode, await stdout, await stderr);
    }
}
