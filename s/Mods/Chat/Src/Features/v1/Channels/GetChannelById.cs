using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Chat.Data;
using FSH.Mods.Chat.Features.v1.Internal;
using FSH.Mods.Chat.Spec;
using FSH.Mods.Chat.Spec.v1;
using FSH.Mods.Chat.Spec.v1.Channel;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Chat.Features.v1.Channels;

public static class GetChannelByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetChannelByIdEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/channels/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new GetChannelByIdQuery(id), ct)))
            .WithName("GetChannelById")
            .WithSummary("Get a single channel with members and unread count")
            .RequirePermission(ChatPermissions.Channels.View);
}

public sealed class GetChannelByIdQueryHandler(
    ChatDbContext db,
    ICurrentUser currentUser)
    : IQueryHandler<GetChannelByIdQuery, ChannelDto>
{
    public async ValueTask<ChannelDto> Handle(GetChannelByIdQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var channel = await db.Channels.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == q.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Channel not found.");

        // Private channels & DMs: must be a member. Public channels: anyone with View can see them.
        if (channel.IsPrivate)
        {
            channel.RequireMember(currentUserId);
        }

        var member = channel.Members.FirstOrDefault(m => string.Equals(m.UserId, currentUserId, StringComparison.Ordinal));
        int unread = 0;
        if (member is not null)
        {
            unread = await db.Messages.AsNoTracking()
                .Where(m => m.ChannelId == channel.Id
                    && m.DeletedAtUtc == null
                    && (member.LastReadMessageId == null || m.Id.CompareTo(member.LastReadMessageId.Value) > 0))
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return channel.ToDto(unread);
    }
}