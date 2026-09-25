using Microsoft.AspNetCore.Antiforgery;

namespace Wissian.Host.Identity;

public static class IdentityEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity");
        group.MapGet("/csrf", (HttpContext context, IAntiforgery csrf) =>
            Results.Ok(new { token = csrf.GetAndStoreTokens(context).RequestToken }));
        group.MapGet("/access-help", () => Results.Ok(new
        {
            mode = "assisted",
            message = "Solicite um link ao responsável pelo acesso. A emissão exige conferência de identidade; nenhum e-mail é enviado automaticamente.",
            lifetimeMinutes = 60
        }));
        group.MapGet("/session", async (HttpContext context, SessionAuthority authority) =>
        {
            var session = await authority.GetAsync(context.User, context.RequestAborted);
            return session is null ? Results.Unauthorized() : Results.Ok(session);
        }).RequireAuthorization();
        var commands = group.MapGroup("").AddEndpointFilter<RequireCsrf>();
        commands.MapPost("/login", (PasswordLogin request, HttpContext context, IdentityFlow flow) => flow.LoginAsync(request, context))
            .RequireRateLimiting("identity");
        commands.MapPost("/mfa", (AuthenticatorLogin request, HttpContext context, IdentityFlow flow) => flow.AuthenticatorAsync(request, context))
            .RequireRateLimiting("identity");
        commands.MapPost("/context", (ContextSelection request, HttpContext context, IdentityFlow flow) => flow.SelectAsync(request, context))
            .RequireAuthorization();
        commands.MapPost("/logout", (HttpContext context, IdentityFlow flow) => flow.LogoutAsync(context)).RequireAuthorization();
        commands.MapPost("/activate", (CompleteAccountAction request, HttpContext context, AccountActions actions) =>
            actions.CompleteAsync(request, activation: true, context)).RequireRateLimiting("identity");
        commands.MapPost("/reset-password", (CompleteAccountAction request, HttpContext context, AccountActions actions) =>
            actions.CompleteAsync(request, activation: false, context)).RequireRateLimiting("identity");
    }

    private sealed class RequireCsrf(IAntiforgery antiforgery) : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            try { await antiforgery.ValidateRequestAsync(context.HttpContext); }
            catch (AntiforgeryValidationException) { return Results.BadRequest(new { code = "csrf_invalid" }); }
            return await next(context);
        }
    }
}
