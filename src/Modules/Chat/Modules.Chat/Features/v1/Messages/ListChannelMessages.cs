using FluentValidation;
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

public static class ListChannelMessagesEndpoint
{
    internal static RouteHandlerBuilder MapListChannelMessagesEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/channels/{id:guid}/messages",
                async (Guid id, Guid? before, int? pageSize, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(
                        new ListChannelMessagesQuery(id, before, pageSize ?? 50),
                        ct)))
            .WithName("ListChannelMessages")
            .WithSummary("List top-level messages in a channel (cursor-paged, reverse chronological)")
            .RequirePermission(ChatPermissions.Channels.View);
}

public sealed class ListChannelMessagesQueryValidator : AbstractValidator<ListChannelMessagesQuery>
{
    public ListChannelMessagesQueryValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}

public sealed class ListChannelMessagesQueryHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IMediator mediator)
    : IQueryHandler<ListChannelMessagesQuery, ReadOnlyCollection<MessageDto>>
{
    public async ValueTask<ReadOnlyCollection<MessageDto>> Handle(
        ListChannelMessagesQuery query,
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

        // Top-level only (no thread replies). Guid v7 monotonic → Id desc = time desc.
        IQueryable<Domain.Message> q = db.Messages
            .Where(m => m.ChannelId == query.ChannelId && m.ParentMessageId == null);

        if (query.Before is { } beforeId)
        {
            q = q.Where(m => m.Id.CompareTo(beforeId) < 0);
        }

        var rows = await q
            .OrderByDescending(m => m.Id)
            .Take(query.PageSize)
            .Include(m => m.Attachments)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = rows.Select(m => m.ToDto()).ToList();
        var resolved = await ChatAttachmentUrls.ResolveAsync(dtos, mediator, cancellationToken).ConfigureAwait(false);
        return resolved.AsReadOnly();
    }
}