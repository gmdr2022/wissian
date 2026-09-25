using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Wissian.Host.Identity;
using Wissian.Persistence;

// M1 local laboratory operator. Issuance for real accounts is deliberately not enabled here.
if (!OperatingSystem.IsWindows() || args.Length != 3 || args[2] != "--synthetic"
    || args[0] is not ("activation" or "recovery") || !Guid.TryParse(args[1], out var userId) || userId == Guid.Empty)
{
    Console.Error.WriteLine("Uso: Wissian.Operator activation|recovery <user-id> --synthetic (Windows, laboratório local).");
    return 2;
}
var builder = WebApplication.CreateBuilder([]);
builder.Logging.ClearProviders();
var configured = builder.Configuration["Wissian:Database"];
if (string.IsNullOrWhiteSpace(configured)) { Console.Error.WriteLine("Configuração runtime ausente."); return 2; }
var connection = new NpgsqlConnectionStringBuilder(configured);
if (connection.Host != "127.0.0.1" || connection.Username != "wissian_runtime"
    || connection.Database?.StartsWith("wissian_", StringComparison.Ordinal) != true
    || !connection.Database.EndsWith("_synthetic", StringComparison.Ordinal))
{
    Console.Error.WriteLine("Emissão permitida somente no laboratório sintético local identificado.");
    return 2;
}
builder.AddWissianIdentity();
await using var app = builder.Build();
await app.Services.GetServices<IHostedService>().OfType<RuntimeSchemaCheck>().Single().StartAsync(CancellationToken.None);
await using var scope = app.Services.CreateAsyncScope();
var link = await AccountLinks.IssueAsync(scope.ServiceProvider.GetRequiredService<UserManager<WissianUser>>(), userId, args[0]);
if (link is null) { Console.Error.WriteLine("Ação indisponível para a identidade informada."); return 3; }

// Restrict a new directory before writing the bearer token. No token is written to stdout or command args.
var directory = new DirectoryInfo(Path.GetFullPath(Path.Combine(".local", "account-actions", Guid.NewGuid().ToString("N"))));
directory.Create();
using var identity = WindowsIdentity.GetCurrent();
var sid = identity.User ?? throw new InvalidOperationException("Identidade Windows indisponível.");
var acl = new DirectorySecurity();
acl.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
acl.SetOwner(sid);
acl.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl,
    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
directory.SetAccessControl(acl);
var path = Path.Combine(directory.FullName, "account-link.json");
await using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
    await JsonSerializer.SerializeAsync(stream, link, new JsonSerializerOptions(JsonSerializerDefaults.Web));
Console.WriteLine(path);
return 0;
