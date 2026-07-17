using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Chat.Contracts.Authorization;
using FSH.Modules.Chat.Contracts.v1.Commands;
using FSH.Modules.Chat.Data;
using FSH.Modules.Chat.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Chat.Features.v1.Channels;

public static class ArchiveChannelEndpoint
{
    internal static RouteHandlerBuilder MapArchiveChannelEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapDelete("/channels/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new ArchiveChannelCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("ArchiveChannel")
            .WithSummary("Soft-archive a channel (channel admin only)")
            .RequirePermission(ChatPermissions.Channels.Create);
}

public sealed class ArchiveChannelCommandValidator : AbstractValidator<ArchiveChannelCommand>
{
    public ArchiveChannelCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
    }
}

public sealed class ArchiveChannelCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<ArchiveChannelCommand, Unit>
{
    public async ValueTask<Unit> Handle(ArchiveChannelCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");

        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == cmd.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Channel not found.");

        channel.RequireAdmin(userId.ToString());

        // Explicit soft-delete (not db.Remove): removing the aggregate cascades Deleted onto
        // the ChannelMember rows, which the audit interceptor does not rescue (they're FK
        // children, not owned), so they'd be hard-deleted and lost on restore.
        channel.Archive(userId.ToString());
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}