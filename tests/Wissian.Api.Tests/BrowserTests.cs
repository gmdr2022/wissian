using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wissian.Core.Identity;
using Wissian.Host.Identity;
using Wissian.Persistence;

namespace Wissian.Api.Tests;

internal static class BrowserTests
{
    public static async Task<int> RunAsync(ApiLab lab, bool administration = false)
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Wissian.slnx"))) root = root.Parent;
        if (root is null) throw new InvalidOperationException("Workspace de testes ausente.");
        var web = Path.Combine(root.FullName, "src", "Wissian.Web");
        if (!File.Exists(Path.Combine(web, "dist", "index.html")))
            throw new InvalidOperationException("Executar npm run build antes do laboratório de navegador.");
        var fixtureRoot = Path.Combine(root.FullName, ".local", "web-fixture");
        await File.WriteAllTextAsync(Path.Combine(fixtureRoot, "private-fixture.json"), "{\"synthetic\":true}");
        await using var fixture = lab.Build(webRoot: fixtureRoot);
        await fixture.StartAsync();
        var address = fixture.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var primary = await lab.PersonAsync(organizationName: "Fultuma Exemplo");
        var secondary = await lab.PersonAsync(organizationName: "Empresa Exemplo");
        var secondMembership = Guid.NewGuid();
        await lab.ChangeAsync(db =>
        {
            db.Memberships.Add(new Membership(secondMembership, primary.UserId, secondary.OrganizationId, lab.Clock.GetUtcNow()));
            db.Permissions.Add(new MembershipPermission(secondary.OrganizationId, secondMembership, "foundation-probe.inspect"));
            return Task.CompletedTask;
        });
        if (administration)
        {
            await CounterpartyTests.EnableAsync(lab, primary);
            await CounterpartyTests.EnableAsync(lab, secondary);
            await CounterpartyTests.EnableAsync(lab, primary with { OrganizationId = secondary.OrganizationId, MembershipId = secondMembership });
        }
        var noMembership = await lab.PersonAsync(membership: false);
        var mfa = await lab.PersonAsync();
        var recovering = await lab.PersonAsync(membership: false);
        var activating = await lab.PersonAsync(membership: false, pending: true);
        await using var scope = fixture.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>();
        var mfaUser = (await users.FindByIdAsync(mfa.UserId.ToString()))!;
        ApiLab.Ensure((await users.ResetAuthenticatorKeyAsync(mfaUser)).Succeeded);
        var mfaKey = await users.GetAuthenticatorKeyAsync(mfaUser);
        ApiLab.Ensure((await users.SetTwoFactorEnabledAsync(mfaUser, true)).Succeeded);
        var recovery = await AccountLinks.IssueAsync(users, recovering.UserId, "recovery");
        var activation = await AccountLinks.IssueAsync(users, activating.UserId, "activation");
        var screenshots = Path.Combine(root.FullName, ".local", "browser-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(screenshots);
        var start = new ProcessStartInfo("node")
        {
            WorkingDirectory = web, UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        start.ArgumentList.Add(Path.Combine(web, "e2e", "browser.mjs"));
        start.Environment["WISSIAN_BROWSER_FIXTURE"] = JsonSerializer.Serialize(new
        {
            baseUrl = address, productionUrl = lab.Address.ToString(), primary, secondary,
            noMembership, mfa, mfaKey, recovering, activating, recovery, activation, screenshots, administration
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        // Synthetic credentials stay in this owned child's environment, not command line/files/logs.
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Navegador não iniciou.");
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(4));
        var errors = process.StandardError.ReadToEndAsync(deadline.Token);
        try
        {
            while (await process.StandardOutput.ReadLineAsync(deadline.Token) is { } line)
            {
                if (!line.StartsWith("WISSIAN_TEST_CONTROL ", StringComparison.Ordinal)) { Console.WriteLine(line); continue; }
                var action = line[21..];
                // No HTTP test-admin route. The owned child's pipe supports only these synthetic mutations.
                object result = await ControlAsync(action, lab, primary, administration);
                await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(result));
                await process.StandardInput.FlushAsync(deadline.Token);
            }
            await process.WaitForExitAsync(deadline.Token);
            var errorText = await errors;
            if (errorText.Length > 0) Console.Error.WriteLine(errorText);
            Console.WriteLine("Browser screenshots: " + screenshots);
            return process.ExitCode;
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await fixture.StopAsync();
        }
    }

    private static async Task<object> ControlAsync(string action, ApiLab lab, SyntheticPerson primary, bool administration)
    {
        await using var db = lab.OwnerDb();
        switch (action)
        {
            case "disable-module":
            case "enable-module":
                var moduleId = administration ? "counterparties" : "foundation-probe";
                var module = await db.OrganizationModules.SingleAsync(m => m.OrganizationId == primary.OrganizationId && m.ModuleId == moduleId);
                db.Entry(module).Property(m => m.IsEnabled).CurrentValue = action == "enable-module";
                break;
            case "disable-write" when administration:
            case "enable-write" when administration:
                var permission = await db.Permissions.SingleAsync(p => p.MembershipId == primary.MembershipId && p.Permission == "counterparties.write");
                db.Entry(permission).Property(p => p.IsActive).CurrentValue = action == "enable-write";
                break;
            case "revoke-membership":
            case "restore-membership":
                var member = await db.Memberships.SingleAsync(m => m.Id == primary.MembershipId);
                db.Entry(member).Property(m => m.IsActive).CurrentValue = action == "restore-membership";
                break;
            case "revoke-sessions":
                foreach (var session in await db.Sessions.Where(s => s.UserId == primary.UserId).ToListAsync())
                    session.Revoke(lab.Clock.GetUtcNow());
                break;
            case "receipt-count":
                return await db.ContextReceipts.CountAsync(r => r.UserId == primary.UserId);
            default: throw new InvalidOperationException("Controle sintético desconhecido.");
        }
        await db.SaveChangesAsync();
        return true;
    }
}
