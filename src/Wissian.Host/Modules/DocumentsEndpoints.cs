using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Wissian.Documents;
using Wissian.Host.Identity;
using Wissian.Persistence;

namespace Wissian.Host.Modules;

public static class DocumentsEndpoints
{
    public static ModuleRegistration Registration { get; } = new(DocumentModule.Manifest, Map, builder =>
    {
        builder.Services.AddDbContext<DocumentsDbContext>((services, options) => options.UseNpgsql(
            services.GetRequiredService<WissianDbContext>().Database.GetDbConnection(), p => p.SetPostgresVersion(18, 6)
                .MigrationsHistoryTable("schema_history", "wissian_documents")));
        builder.Services.AddScoped<DocumentStore>();
        builder.Services.AddSingleton<IAttachmentInspector, WindowsAttachmentInspector>();
        builder.Services.AddSingleton<DocumentTransferSlots>();
        builder.Services.AddHostedService<DocumentsSchemaCheck>();
    });
    private static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/organizations/{organizationId:guid}/documents").RequireAuthorization();
        group.MapGet("", (Guid organizationId, string? q, string? status, int? page, HttpContext context) =>
            Execute(context, organizationId, false, null, async (store, _) => Results.Ok(
                await store.SearchAsync(organizationId, q, status ?? "active", page ?? 1, context.RequestAborted))));
        group.MapGet("/{id:guid}", (Guid organizationId, Guid id, HttpContext context) =>
            Execute(context, organizationId, false, null, async (store, _) =>
                await store.GetAsync(organizationId, id, context.RequestAborted) is { } item ? Results.Ok(item) : Results.NotFound()));
        group.MapGet("/{id:guid}/revisions/{revision:int}/download", Download);
        group.MapPost("/uploads", Upload).AddEndpointFilter<RequireCsrf>();
        group.MapPost("/commands", Command).AddEndpointFilter<RequireCsrf>();
    }
    private static async Task<IResult> Upload(Guid organizationId, HttpContext context)
    {
        var services = context.RequestServices;
        var authority = services.GetRequiredService<SessionAuthority>();
        if (!await authority.CanReadAsync(context.User, organizationId, "documents", "documents.write", context.RequestAborted))
            return Results.NotFound();
        var slots = services.GetRequiredService<DocumentTransferSlots>();
        if (!slots.TryEnter()) return Results.StatusCode(429);
        try
        {
            var (command, file) = await DocumentUpload.ReadAsync(context);
            var scan = await services.GetRequiredService<IAttachmentInspector>().InspectAsync(file.Bytes,
                Path.GetExtension(file.FileName).ToLowerInvariant(), context.RequestAborted);
            if (scan.State == "blocked") return Results.UnprocessableEntity(new { code = "attachment_blocked" });
            return await Apply(context, organizationId, command, file, scan);
        }
        catch (ArgumentException) { return Invalid(); }
        catch (JsonException) { return Invalid(); }
        catch (InvalidDataException) { return Invalid(); }
        catch (BadHttpRequestException error) { return Results.StatusCode(error.StatusCode); }
        finally { slots.Exit(); }
    }
    private static async Task<IResult> Command(Guid organizationId, DocumentCommand request, HttpContext context)
    {
        try { request = DocumentStore.Normalize(request, false); }
        catch (ArgumentException) { return Invalid(); }
        if (request.Action != "scan") return await Apply(context, organizationId, request, null, null);
        var slots = context.RequestServices.GetRequiredService<DocumentTransferSlots>();
        if (!slots.TryEnter()) return Results.StatusCode(429);
        try
        {
            DocumentFile? file = null;
            // Read under authority, then release locks before invoking a native scanner. Immutable bytes
            // and expected document/session versions bind the subsequent freshly authorized command.
            var read = await Execute(context, organizationId, true, request.ExpectedSessionVersion, async (store, _) =>
            {
                file = await store.FileAsync(organizationId, request.DocumentId!.Value, request.Revision!.Value, true, Now(context), context.RequestAborted);
                return file is null ? Results.NotFound() : Results.Ok();
            });
            if (file is null) return read;
            var scan = await context.RequestServices.GetRequiredService<IAttachmentInspector>().InspectAsync(file.Bytes,
                Path.GetExtension(file.Revision.FileName).ToLowerInvariant(), context.RequestAborted);
            return await Apply(context, organizationId, request, null, scan);
        }
        finally { slots.Exit(); }
    }
    private static async Task Download(Guid organizationId, Guid id, int revision, HttpContext context)
    {
        var slots = context.RequestServices.GetRequiredService<DocumentTransferSlots>();
        if (!slots.TryEnter()) { context.Response.StatusCode = 429; return; }
        try
        {
            var result = await Execute(context, organizationId, false, null, async (store, scope) =>
            {
                var file = await store.FileAsync(organizationId, id, revision, false, Now(context), context.RequestAborted);
                if (file is null) return Results.NotFound();
                context.RequestServices.GetRequiredService<SessionAuthority>().Record(scope, "documents.download", "document", id, context);
                context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; sandbox";
                context.Response.Headers.XContentTypeOptions = "nosniff";
                return Results.File(file.Bytes, file.Revision.MediaType,
                    $"{id:N}-v{revision}{Path.GetExtension(file.Revision.FileName).ToLowerInvariant()}", enableRangeProcessing: false);
            });
            await result.ExecuteAsync(context);
        }
        finally { slots.Exit(); }
    }
    private static Task<IResult> Apply(HttpContext context, Guid organization, DocumentCommand request,
        ValidatedAttachment? file, AttachmentInspection? scan) =>
        Execute(context, organization, true, request.ExpectedSessionVersion, async (store, scope) =>
        {
            var result = await store.ApplyAsync(organization, scope.ActorId, request, file, scan, Now(context), context.RequestAborted);
            if (result is null) return Results.NotFound();
            if (!result.Replayed) context.RequestServices.GetRequiredService<SessionAuthority>().Record(scope,
                "documents." + request.Action, "document", result.Id, context);
            return Results.Ok(result);
        });
    private static DateTimeOffset Now(HttpContext context) => context.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();
    private static IResult Invalid() => Results.BadRequest(new { code = "document_invalid" });
    private static Task<IResult> Execute(HttpContext context, Guid organization, bool write, long? version,
        Func<DocumentStore, AuthorizedScope, Task<IResult>> action)
    {
        var services = context.RequestServices;
        return services.GetRequiredService<SessionAuthority>().RunAsync(context, organization, "documents",
            write ? "documents.write" : "documents.read", version, async scope =>
            {
                var core = services.GetRequiredService<WissianDbContext>();
                var module = services.GetRequiredService<DocumentsDbContext>();
                await using var shared = await module.Database.UseTransactionAsync(core.Database.CurrentTransaction!.GetDbTransaction(), context.RequestAborted);
                try { return await action(services.GetRequiredService<DocumentStore>(), scope); }
                catch (ArgumentException) { return Invalid(); }
                catch (DocumentConflictException) { return Results.Conflict(new { code = "resource_changed" }); }
                catch (DocumentStateException) { return Results.Conflict(new { code = "document_state" }); }
                catch (DocumentLimitException) { return Results.Conflict(new { code = "document_limit" }); }
                catch (DbUpdateException error) when (error.InnerException is PostgresException { ConstraintName: "document_quota" })
                { return Results.Conflict(new { code = "document_limit" }); }
            });
    }
}

public sealed class DocumentsSchemaCheck(IServiceScopeFactory scopes) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        var ownsSchema = await db.Database.SqlQueryRaw<bool>("""
            SELECT EXISTS(SELECT 1 FROM pg_namespace n JOIN pg_roles r ON r.oid = n.nspowner
                WHERE n.nspname = 'wissian_documents' AND r.rolname = current_user)
                OR EXISTS (SELECT 1 FROM pg_class t JOIN pg_namespace n ON n.oid = t.relnamespace
                    JOIN pg_roles r ON r.oid = t.relowner
                    WHERE n.nspname = 'wissian_documents' AND r.rolname = current_user) AS "Value"
            """).SingleAsync(cancellationToken);
        if (ownsSchema || !db.Database.GetMigrations().SequenceEqual(await db.Database.GetAppliedMigrationsAsync(cancellationToken)))
            throw new InvalidOperationException("Schema do módulo documentos incompatível ou credencial privilegiada.");
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
