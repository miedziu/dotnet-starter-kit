using FSH.Framework.Web.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Identity.Features.v1.Users;

public static class SelfRegisterUserEndpoint
{
    internal static RouteHandlerBuilder MapSelfRegisterUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/self-register", async (RegisterUserCommand command,
            HttpContext context,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var origin = $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase.Value}";
            command.Origin = origin;
            var result = await mediator.Send(command, ct);
            return TypedResults.Created($"/api/v1/identity/users/{result.UserId}", result);
        })
        .WithName("SelfRegisterUser")
        .WithSummary("Self register user")
        .WithDescription("Allow a user to self-register. Anonymous;")
        .AllowAnonymous()
        .WithIdempotency()
        .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}