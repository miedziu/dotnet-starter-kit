using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.File.Data;
using FSH.Mods.File.Spec;
using FSH.Mods.File.Spec.v1.File;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.File.Features.v1;

public static class RestoreFileEndpoint
{
    internal static RouteHandlerBuilder MapRestoreFileEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/{id:guid}/restore",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new RestoreFileCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("RestoreFile")
            .WithSummary("Restore a soft-deleted file from trash (admin)")
            .RequirePermission(FilePermissions.Restore);
}

public sealed class RestoreFileCommandValidator : AbstractValidator<RestoreFileCommand>
{
    public RestoreFileCommandValidator()
    {
        RuleFor(x => x.FileAssetId).NotEmpty();
    }
}

public sealed class RestoreFileCommandHandler(FileDbContext db)
    : ICommandHandler<RestoreFileCommand, Unit>
{
    public async ValueTask<Unit> Handle(RestoreFileCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // IgnoreQueryFilters because the SoftDelete filter would otherwise hide the row.
        var f = await db.FileAssets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == cmd.FileAssetId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("file not found");

        if (f.DeletedAt == null)
        {
            return Unit.Value; // idempotent — already live
        }

        f.Restore();
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}