using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Chat.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Chat.Features.v1.Channels;

public static class RestoreChannelEndpoint
{
    internal static RouteHandlerBuilder MapRestoreChannelEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/channels/{id:guid}/restore",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new RestoreChannelCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("RestoreChannel")
            .WithSummary("Restore an archived channel (admin moderation)")
            .RequirePermission(ChatPermissions.Channels.ManageAll);
}

public sealed class RestoreChannelCommandValidator : AbstractValidator<RestoreChannelCommand>
{
    public RestoreChannelCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
    }
}

public sealed class RestoreChannelCommandHandler(ChatDbContext db)
    : ICommandHandler<RestoreChannelCommand, Unit>
{
    public async ValueTask<Unit> Handle(RestoreChannelCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // IgnoreQueryFilters bypasses the SoftDelete filter so we can find an archived channel.
        var channel = await db.Channels.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == cmd.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Channel not found.");

        if (!channel.IsDeleted) return Unit.Value; // idempotent
        channel.Restore();
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}