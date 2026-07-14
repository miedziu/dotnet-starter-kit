using FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep3;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users.RegisterUserStep3;

public static class RegisterUserStep3Endpoint
{
    internal static RouteHandlerBuilder MapRegisterUserStep3Endpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/register/step3", async (RegisterUserStep3Command command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("RegisterUserStep3")
        .WithSummary("Register user - Step 3 (profile)")
        .WithDescription("Update user profile information. Requires authentication.")
        .Produces<RegisterUserStep3Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}