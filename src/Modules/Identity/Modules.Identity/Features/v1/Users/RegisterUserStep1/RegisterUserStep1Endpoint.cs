using FSH.Framework.Web.Idempotency;
using FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep1;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users.RegisterUserStep1;

public static class RegisterUserStep1Endpoint
{
    internal static RouteHandlerBuilder MapRegisterUserStep1Endpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/register/step1", async (RegisterUserStep1Command command,
            HttpContext context,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var origin = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase.Value}";
            command.Origin = origin;
            var result = await mediator.Send(command, ct);
            return TypedResults.Created($"/api/v1/identity/users/{result.UserId}", result);
        })
        .WithName("RegisterUserStep1")
        .WithSummary("Register user - Step 1 (email and password)")
        .AllowAnonymous()
        .WithIdempotency()
        .WithDescription("Create a new user account with email and password. User must confirm email before proceeding to next steps.")
        .Produces<RegisterUserStep1Response>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}