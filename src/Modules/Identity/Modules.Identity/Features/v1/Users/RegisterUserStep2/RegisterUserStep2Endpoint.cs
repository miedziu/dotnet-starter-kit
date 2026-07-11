using FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep2;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users.RegisterUserStep2;

public static class RegisterUserStep2Endpoint
{
    internal static RouteHandlerBuilder MapRegisterUserStep2Endpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/register/step2", async (RegisterUserStep2Command command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("RegisterUserStep2")
        .WithSummary("Register user - Step 2 (address)")
        .WithDescription("Update user address information. Requires authentication.")
        .Produces<RegisterUserStep2Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}