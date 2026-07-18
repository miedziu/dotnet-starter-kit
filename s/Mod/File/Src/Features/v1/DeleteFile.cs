using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.File.Data;
using FSH.Mod.File.Services;
using FSH.Mod.File.Spec;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.File.Features.v1;

public static class DeleteFileEndpoint
{
    internal static RouteHandlerBuilder MapDeleteFileEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapDelete("/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteFileCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteFile")
            .WithSummary("Soft-delete a file; bytes purged after retention window")
            .RequirePermission(FilePermissions.DeleteOwn);
}

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.FileAssetId).NotEmpty();
    }
}

public sealed class DeleteFileCommandHandler(
    FileDbContext db,
    FileAccessPolicyRegistry policies,
    ICurrentUser currentUser)
    : ICommandHandler<DeleteFileCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteFileCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var f = await db.FileAssets
            .FirstOrDefaultAsync(x => x.Id == cmd.FileAssetId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("file not found");

        var userId = currentUser.GetUserId().ToString();
        var policy = policies.Resolve(f.OwnerType)
            ?? throw new ForbiddenException("no policy");
        var ctx = new FileAccessContext(f.Id, f.OwnerType, f.OwnerId, f.CreatedByUserId, (int)f.Visibility);
        if (!await policy.CanDeleteAsync(ctx, userId, cancellationToken).ConfigureAwait(false))
        {
            throw new ForbiddenException("not allowed to delete this file");
        }

        // Soft-delete: AuditableEntitySaveChangesInterceptor sets IsDeleted/DeletedAt/DeletedBy on
        // Remove() for ISoftDeletable; byte purge runs later via PurgeDeletedFilesJob post-retention.
        db.FileAssets.Remove(f);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}