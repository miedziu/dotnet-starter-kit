using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Claims;
using FSH.Framework.Storage;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace FSH.Modules.Identity.Features.v1.Users;

public static class UpdateUserEndpoint
{
    internal static RouteHandlerBuilder MapUpdateUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/profile", async ([FromBody] UpdateUserCommand request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            if (user.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            // Force the target id to the authenticated user — this endpoint is for self-update
            // only, regardless of any id the caller supplied in the body.
            request.Id = userId;

            await mediator.Send(request, ct);
            return TypedResults.Ok();
        })
        .WithName("UpdateUserProfile")
        .WithSummary("Update user profile")
        .RequireAuthorization()
        .WithDescription("Update profile details for the authenticated user. Any signed-in user may edit their own profile; no admin permission required.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(15)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        When(x => x.Image is not null, () =>
        {
            RuleFor(x => x.Image!)
                .SetValidator(new UserImageValidator(FileType.Image));
        });

        // Prevent deleting and uploading image at the same time
        RuleFor(x => x)
            .Must(x => !(x.DeleteCurrentImage && x.Image is not null))
            .WithMessage("You cannot upload a new image and delete the current one simultaneously.");
    }
}

public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, Unit>
{
    private readonly IUserService _userService;

    public UpdateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<Unit> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        await _userService.UpdateAsync(
            command.Id,
            command.FirstName ?? string.Empty,
            command.LastName ?? string.Empty,
            command.PhoneNumber ?? string.Empty,
            command.Image!,
            command.DeleteCurrentImage,
            cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}