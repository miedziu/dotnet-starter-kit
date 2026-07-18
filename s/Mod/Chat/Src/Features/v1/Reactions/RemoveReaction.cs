using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Realtime;
using FSH.Mod.Chat.Data;
using FSH.Mod.Chat.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Chat.Features.v1.Reactions;

public static class RemoveReactionEndpoint
{
    internal static RouteHandlerBuilder MapRemoveReactionEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapDelete("/messages/{id:guid}/reactions/{emoji}",
                async (Guid id, string emoji, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new RemoveReactionCommand(id, Uri.UnescapeDataString(emoji)), ct);
                    return Results.NoContent();
                })
            .WithName("RemoveReaction")
            .WithSummary("Toggle off a reaction emoji on a message")
            .RequirePermission(ChatPermissions.Messages.Send);
}

public sealed class RemoveReactionCommandValidator : AbstractValidator<RemoveReactionCommand>
{
    public RemoveReactionCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.Emoji).NotEmpty().MaximumLength(64);
    }
}

public sealed class RemoveReactionCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IHubContext<AppHub> hub)
    : ICommandHandler<RemoveReactionCommand, Unit>
{
    public async ValueTask<Unit> Handle(RemoveReactionCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == cmd.MessageId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Message not found.");

        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == message.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Message not found.");
        channel.RequireMember(currentUserId);

        if (!message.RemoveReaction(currentUserId, cmd.Emoji))
        {
            // Already absent — idempotent no-op, no broadcast.
            return Unit.Value;
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await hub.Clients.Group($"channel:{channel.Id}")
            .SendAsync("ChatReactionChanged",
                new { channelId = channel.Id, messageId = message.Id, userId = currentUserId, emoji = cmd.Emoji.Trim(), kind = "removed" },
                cancellationToken)
            .ConfigureAwait(false);
        return Unit.Value;
    }
}