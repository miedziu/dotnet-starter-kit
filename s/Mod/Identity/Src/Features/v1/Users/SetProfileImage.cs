using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Mod.Identity.Spec.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Identity.Features.v1.Users;

public static class SetProfileImageEndpoint
{
    internal static RouteHandlerBuilder MapSetProfileImageEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPut("/profile/image",
                async (SetProfileImageCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("SetProfileImage")
            .WithSummary("Set the authenticated user's avatar URL")
            .WithDescription("Persists a durable image URL on the current user's profile. Typically called after the File module's presigned-upload flow returns a publicUrl. Pass a null/empty body to clear.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
}

public sealed class SetProfileImageCommandValidator : AbstractValidator<SetProfileImageCommand>
{
    public SetProfileImageCommandValidator()
    {
        // Empty/null is allowed (clears the image). When set, must look like a URL or relative path.
        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
    }
}

public sealed class SetProfileImageCommandHandler(
    IUserProfileService profileService,
    ICurrentUser currentUser)
    : ICommandHandler<SetProfileImageCommand, Unit>
{
    public async ValueTask<Unit> Handle(SetProfileImageCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedException("no current user");
        }

        await profileService
            .SetImageUrlAsync(userId.ToString(), command.ImageUrl, cancellationToken)
            .ConfigureAwait(false);

        return Unit.Value;
    }
}