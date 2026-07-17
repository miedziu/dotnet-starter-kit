using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Realtime;
using FSH.Modules.Chat.Contracts.Authorization;
using FSH.Modules.Chat.Contracts.v1.Commands;
using FSH.Modules.Chat.Data;
using FSH.Modules.Chat.Features.v1.Internal;
using FSH.Modules.Identity.Contracts.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Chat.Features.v1.Messages;

public static class DeleteMessageEndpoint
{
    internal static RouteHandlerBuilder MapDeleteMessageEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapDelete("/messages/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteMessageCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteMessage")
            .WithSummary("Delete a message — author can delete own; moderators can delete any")
            .RequirePermission(ChatPermissions.Messages.DeleteOwn);
}

public sealed class DeleteMessageCommandValidator : AbstractValidator<DeleteMessageCommand>
{
    public DeleteMessageCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
    }
}

public sealed class DeleteMessageCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IUserPermissionService permissions,
    IHubContext<AppHub> hub)
    : ICommandHandler<DeleteMessageCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteMessageCommand cmd, CancellationToken cancellationToken)
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

        bool isModerator = await permissions
            .HasPermissionAsync(currentUserId, ChatPermissions.Messages.DeleteAny, cancellationToken)
            .ConfigureAwait(false);

        message.SoftDelete(currentUserId, isModerator);

        // If this was a thread reply, decrement the parent's ReplyCount.
        if (message.ParentMessageId is { } parentId)
        {
            var parent = await db.Messages.FirstOrDefaultAsync(m => m.Id == parentId, cancellationToken)
                .ConfigureAwait(false);
            parent?.DecrementReplyCount();
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await hub.Clients.Group($"channel:{channel.Id}")
            .SendAsync("ChatMessageDeleted", new { channelId = channel.Id, messageId = message.Id }, cancellationToken)
            .ConfigureAwait(false);
        return Unit.Value;
    }
}