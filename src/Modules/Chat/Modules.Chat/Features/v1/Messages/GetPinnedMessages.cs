using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Chat.Contracts.Authorization;
using FSH.Modules.Chat.Contracts.v1.DTOs;
using FSH.Modules.Chat.Contracts.v1.Queries;
using FSH.Modules.Chat.Data;
using FSH.Modules.Chat.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FSH.Modules.Chat.Features.v1.Messages;

public static class GetPinnedMessagesEndpoint
{
    internal static RouteHandlerBuilder MapGetPinnedMessagesEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/channels/{id:guid}/pinned",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    var result = await mediator.Send(new GetPinnedMessagesQuery(id), ct);
                    return Results.Ok(result);
                })
            .WithName("ListPinnedMessages")
            .WithSummary("Pinned messages in a channel — most recently pinned first")
            .RequirePermission(ChatPermissions.Channels.View);
}

public sealed class GetPinnedMessagesQueryHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IMediator mediator)
    : IQueryHandler<GetPinnedMessagesQuery, ReadOnlyCollection<MessageDto>>
{
    public async ValueTask<ReadOnlyCollection<MessageDto>> Handle(
        GetPinnedMessagesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == query.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Channel not found.");
        channel.RequireMember(currentUserId);

        var rows = await db.Messages
            .Where(m => m.ChannelId == query.ChannelId && m.IsPinned)
            .OrderByDescending(m => m.PinnedAtUtc)
            .Include(m => m.Attachments)
            .Include(m => m.Mentions)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = rows.Select(m => m.ToDto()).ToList();
        var resolved = await ChatAttachmentUrls.ResolveAsync(dtos, mediator, cancellationToken).ConfigureAwait(false);
        return resolved.AsReadOnly();
    }
}