using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Realtime;
using FSH.Mods.Chat.Data;
using FSH.Mods.Chat.Features.v1.Internal;
using FSH.Mods.Chat.Spec;
using FSH.Mods.Chat.Spec.v1.Member;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Chat.Features.v1.Channels;

public static class RemoveChannelMemberEndpoint
{
    internal static RouteHandlerBuilder MapRemoveChannelMemberEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapDelete("/channels/{id:guid}/members/{userId}",
                async (Guid id, string userId, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new RemoveChannelMemberCommand(id, userId), ct);
                    return Results.NoContent();
                })
            .WithName("RemoveChannelMember")
            .WithSummary("Remove a member (admin) or leave the channel (self)")
            .RequirePermission(ChatPermissions.Channels.View);
}

public sealed class RemoveChannelMemberCommandValidator : AbstractValidator<RemoveChannelMemberCommand>
{
    public RemoveChannelMemberCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(64);
    }
}

public sealed class RemoveChannelMemberCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IHubContext<AppHub> hub)
    : ICommandHandler<RemoveChannelMemberCommand, Unit>
{
    public async ValueTask<Unit> Handle(RemoveChannelMemberCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == cmd.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Channel not found.");

        // Self-leave is always allowed for the current user. Removing someone else requires Admin.
        var isSelfLeave = string.Equals(cmd.UserId, currentUserId, StringComparison.Ordinal);
        if (!isSelfLeave)
        {
            channel.RequireAdmin(currentUserId);
        }
        else
        {
            channel.RequireMember(currentUserId);
        }

        channel.RemoveMember(cmd.UserId, currentUserId);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await hub.Clients.Group($"channel:{channel.Id}")
            .SendAsync("ChatChannelMemberRemoved", new { channelId = channel.Id, userId = cmd.UserId }, cancellationToken)
            .ConfigureAwait(false);
        await hub.Clients.Group($"user:{cmd.UserId}")
            .SendAsync("ChatChannelRemoved", new { channelId = channel.Id }, cancellationToken)
            .ConfigureAwait(false);
        return Unit.Value;
    }
}