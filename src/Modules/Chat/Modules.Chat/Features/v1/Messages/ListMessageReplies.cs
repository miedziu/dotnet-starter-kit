using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Chat.Contracts.Authorization;
using FSH.Modules.Chat.Contracts.v1.Dtos;
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

public static class ListMessageRepliesEndpoint
{
    internal static RouteHandlerBuilder MapListMessageRepliesEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/messages/{id:guid}/replies",
                async (Guid id, Guid? before, int? pageSize, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(
                        new ListMessageRepliesQuery(id, before, pageSize ?? 50),
                        ct)))
            .WithName("ListMessageReplies")
            .WithSummary("List replies to a thread parent message (newest first, cursor-paged)")
            .RequirePermission(ChatPermissions.Channels.View);
}

public sealed class ListMessageRepliesQueryValidator : AbstractValidator<ListMessageRepliesQuery>
{
    public ListMessageRepliesQueryValidator()
    {
        RuleFor(x => x.ParentMessageId).NotEmpty();
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}

public sealed class ListMessageRepliesQueryHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IMediator mediator)
    : IQueryHandler<ListMessageRepliesQuery, ReadOnlyCollection<MessageDto>>
{
    public async ValueTask<ReadOnlyCollection<MessageDto>> Handle(
        ListMessageRepliesQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        // Load the parent so we can authorize the caller through the channel.
        var parent = await db.Messages
            .Where(m => m.Id == query.ParentMessageId)
            .Select(m => new { m.Id, m.ChannelId })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Parent message not found.");

        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == parent.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Parent message not found.");
        channel.RequireMember(currentUserId);

        IQueryable<Domain.Message> q = db.Messages
            .Where(m => m.ParentMessageId == query.ParentMessageId);

        if (query.Before is { } beforeId)
        {
            q = q.Where(m => m.Id.CompareTo(beforeId) < 0);
        }

        var rows = await q
            .OrderByDescending(m => m.Id)
            .Take(query.PageSize)
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