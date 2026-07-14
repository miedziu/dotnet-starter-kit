using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Realtime;
using FSH.Modules.Chat.Contracts.Authorization;
using FSH.Modules.Chat.Contracts.v1.Commands;
using FSH.Modules.Chat.Data;
using FSH.Modules.Chat.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Chat.Features.v1.Reactions;

public static class AddReactionEndpoint
{
    internal static RouteHandlerBuilder MapAddReactionEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/messages/{id:guid}/reactions",
                async (Guid id, [FromBody] AddReactionBody body, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new AddReactionCommand(id, body.Emoji), ct);
                    return Results.NoContent();
                })
            .WithName("AddReaction")
            .WithSummary("Toggle on a reaction emoji on a message")
            .RequirePermission(ChatPermissions.Messages.Send);

    public sealed record AddReactionBody(string Emoji);
}

public sealed class AddReactionCommandValidator : AbstractValidator<AddReactionCommand>
{
    public AddReactionCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.Emoji).NotEmpty().MaximumLength(64);
    }
}

public sealed class AddReactionCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IHubContext<AppHub> hub)
    : ICommandHandler<AddReactionCommand, Unit>
{
    public async ValueTask<Unit> Handle(AddReactionCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == cmd.MessageId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Message not found.");

        // Authorize through the parent channel — don't leak existence to non-members.
        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == message.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Message not found.");
        channel.RequireMember(currentUserId);

        var added = message.AddReaction(currentUserId, cmd.Emoji);
        if (added is null)
        {
            // Already reacted — idempotent no-op, no broadcast either.
            return Unit.Value;
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await hub.Clients.Group($"channel:{channel.Id}")
            .SendAsync("ChatReactionChanged",
                new { channelId = channel.Id, messageId = message.Id, userId = currentUserId, emoji = added.Emoji, kind = "added" },
                cancellationToken)
            .ConfigureAwait(false);
        return Unit.Value;
    }
}