using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wissian.Counterparties;
using Wissian.Host.Identity;
using Wissian.Persistence;

namespace Wissian.Host.Modules;

public static class CounterpartiesEndpoints
{
    public static ModuleRegistration Registration { get; } = new(CounterpartyModule.Manifest, Map, builder =>
    {
        builder.Services.AddDbContext<CounterpartiesDbContext>((services, options) => options.UseNpgsql(
            services.GetRequiredService<WissianDbContext>().Database.GetDbConnection(), p => p.SetPostgresVersion(18, 6)
                .MigrationsHistoryTable("schema_history", "wissian_counterparties")));
        builder.Services.AddScoped<CounterpartyStore>();
        builder.Services.AddHostedService<CounterpartiesSchemaCheck>();
    });

    private static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/organizations/{organizationId:guid}/counterparties").RequireAuthorization();
        group.MapGet("", (Guid organizationId, string? q, string? status, int? page, HttpContext context) =>
            Execute(context, organizationId, false, null, async (store, _) =>
                Results.Ok(await store.SearchAsync(organizationId, q, status ?? "active", page ?? 1, context.RequestAborted))));
        group.MapGet("/{id:guid}", (Guid organizationId, Guid id, HttpContext context) =>
            Execute(context, organizationId, false, null, async (store, _) =>
                await store.GetAsync(organizationId, id, context.RequestAborted) is { } item ? Results.Ok(item) : Results.NotFound()));
        group.MapPost("", (Guid organizationId, CounterpartyCommand request, HttpContext context) =>
            Apply(context, organizationId, null, request)).AddEndpointFilter<RequireCsrf>();
        group.MapPost("/{id:guid}", (Guid organizationId, Guid id, CounterpartyCommand request, HttpContext context) =>
            Apply(context, organizationId, id, request)).AddEndpointFilter<RequireCsrf>();
    }

    private static Task<IResult> Apply(HttpContext context, Guid organization, Guid? id, CounterpartyCommand request) =>
        Execute(context, organization, true, request.ExpectedSessionVersion, async (store, authorityScope) =>
        {
            var result = await store.ApplyAsync(organization, authorityScope.ActorId, id, request,
                context.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow(), context.RequestAborted);
            if (result is null) return Results.NotFound();
            if (!result.Replayed) context.RequestServices.GetRequiredService<SessionAuthority>().Record(authorityScope,
                "counterparties." + request.Action, "counterparty", result.Id, context);
            return Results.Ok(result);
        });

    private static Task<IResult> Execute(HttpContext context, Guid organization, bool write, long? version,
        Func<CounterpartyStore, AuthorizedScope, Task<IResult>> action)
    {
        var services = context.RequestServices;
        var authority = services.GetRequiredService<SessionAuthority>();
        return authority.RunAsync(context, organization, "counterparties", write ? "counterparties.write" : "counterparties.read", version,
            async scope =>
            {
                var core = services.GetRequiredService<WissianDbContext>();
                var module = services.GetRequiredService<CounterpartiesDbContext>();
                await using var shared = await module.Database.UseTransactionAsync(core.Database.CurrentTransaction!.GetDbTransaction(), context.RequestAborted);
                try { return await action(services.GetRequiredService<CounterpartyStore>(), scope); }
                catch (ArgumentException) { return Results.BadRequest(new { code = "counterparty_invalid" }); }
                catch (CounterpartyConflictException) { return Results.Conflict(new { code = "resource_changed" }); }
                catch (CounterpartyStateException)
                { return Results.Conflict(new { code = "counterparty_state" }); }
            });
    }
}

public sealed class CounterpartiesSchemaCheck(IServiceScopeFactory scopes) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CounterpartiesDbContext>();
        var ownsSchema = await db.Database.SqlQueryRaw<bool>("""
            SELECT EXISTS(SELECT 1 FROM pg_namespace n JOIN pg_roles r ON r.oid = n.nspowner
                WHERE n.nspname = 'wissian_counterparties' AND r.rolname = current_user)
                OR EXISTS (SELECT 1 FROM pg_class t JOIN pg_namespace n ON n.oid = t.relnamespace
                    JOIN pg_roles r ON r.oid = t.relowner
                    WHERE n.nspname = 'wissian_counterparties' AND r.rolname = current_user) AS "Value"
            """).SingleAsync(cancellationToken);
        if (ownsSchema || !db.Database.GetMigrations().SequenceEqual(await db.Database.GetAppliedMigrationsAsync(cancellationToken)))
            throw new InvalidOperationException("Schema do módulo contrapartes incompatível ou credencial privilegiada.");
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
