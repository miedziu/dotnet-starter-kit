using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users.ConfirmEmail;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class ConfirmEmailEndpoint
{
    internal static RouteHandlerBuilder MapConfirmEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/confirm-email", async (string userId, string code, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ConfirmEmailCommand(userId, code), ct);
            return TypedResults.Ok(result);
        })
        .WithName("ConfirmEmail")
        .WithSummary("Confirm user email")
        .WithDescription("Confirm a user's email address.")
        .AllowAnonymous()
        .Produces(StatusCodes.Status200OK);
    }
}

public sealed class ConfirmEmailCommandHandler : ICommandHandler<ConfirmEmailCommand, string>
{
    private readonly IUserService _userService;

    public ConfirmEmailCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<string> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await _userService.ConfirmEmailAsync(command.UserId, command.Code, cancellationToken)
            .ConfigureAwait(false);
    }
}