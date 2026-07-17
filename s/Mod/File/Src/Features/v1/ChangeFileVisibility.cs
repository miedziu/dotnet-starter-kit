using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Storage.Services;
using FSH.Modules.Files.Contracts;
using FSH.Modules.Files.Contracts.Authorization;
using FSH.Modules.Files.Contracts.v1.Commands;
using FSH.Modules.Files.Contracts.v1.Dtos;
using FSH.Modules.Files.Data;
using FSH.Modules.Files.Features.v1.Internal;
using FSH.Modules.Files.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Files.Features.v1;

public sealed record ChangeFileVisibilityRequest(Visibility Visibility);

public static class ChangeFileVisibilityEndpoint
{
    internal static RouteHandlerBuilder MapChangeFileVisibilityEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPatch("/{id:guid}/visibility",
                async (Guid id, ChangeFileVisibilityRequest body, IMediator mediator, CancellationToken ct) =>
                {
                    var dto = await mediator.Send(new ChangeFileVisibilityCommand(id, body.Visibility), ct);
                    return Results.Ok(dto);
                })
            .WithName("ChangeFileVisibility")
            .WithSummary("Flip a file's visibility (Public ↔ Private)")
            .WithDescription("Authenticated upload permission gates the HTTP surface; per-file authorization is delegated to the OwnerType's IFileAccessPolicy (uploader-only by default).")
            // Upload is a basic permission so every authenticated user has it; the per-file
            // policy check inside the handler refines who can actually flip the bit.
            .RequirePermission(FilesPermissions.Upload)
            .Produces<FileAssetDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
}

public sealed class ChangeFileVisibilityCommandValidator : AbstractValidator<ChangeFileVisibilityCommand>
{
    public ChangeFileVisibilityCommandValidator()
    {
        RuleFor(x => x.FileAssetId).NotEmpty();

        RuleFor(x => x.Visibility)
            .IsInEnum()
            .WithMessage("Visibility must be Public or Private.");
    }
}

public sealed class ChangeFileVisibilityCommandHandler(
    FilesDbContext db,
    FileAccessPolicyRegistry policies,
    ICurrentUser currentUser,
    IStorageService storage)
    : ICommandHandler<ChangeFileVisibilityCommand, FileAssetDto>
{
    public async ValueTask<FileAssetDto> Handle(ChangeFileVisibilityCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (cmd.Visibility is not (Visibility.Public or Visibility.Private))
        {
            throw new CustomException(
                $"Unknown visibility value '{cmd.Visibility}'.",
                errors: null,
                System.Net.HttpStatusCode.BadRequest);
        }

        var f = await db.FileAssets
            .FirstOrDefaultAsync(x => x.Id == cmd.FileAssetId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("file not found");

        var userId = currentUser.GetUserId().ToString();
        var policy = policies.Resolve(f.OwnerType)
            ?? throw new ForbiddenException("no policy");
        var ctx = new FileAccessContext(f.Id, f.OwnerType, f.OwnerId, f.CreatedByUserId, (int)f.Visibility);
        if (!await policy.CanChangeVisibilityAsync(ctx, userId, cancellationToken).ConfigureAwait(false))
        {
            throw new ForbiddenException("not allowed to change this file's visibility");
        }

        f.ChangeFileVisibility(cmd.Visibility);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var publicUrl = f.Visibility == Visibility.Public
            ? storage.BuildPublicUrl(f.StorageKey)
            : null;
        return FileAssetMapper.ToDto(f, publicUrl);
    }
}