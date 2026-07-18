using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Chat.Data;
using FSH.Mod.Chat.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FSH.Mod.Chat.Features.v1.Channels;

public static class DiscoverChannelsEndpoint
{
    internal static RouteHandlerBuilder MapDiscoverChannelsEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/channels/discover",
                async (string? search, int? page, int? pageSize, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new DiscoverChannelsQuery(search, page ?? 1, pageSize ?? 50), ct)))
            .WithName("DiscoverChannels")
            .WithSummary("List public channels the current user is NOT yet in")
            .RequirePermission(ChatPermissions.Channels.View);
}

public sealed class DiscoverChannelsQueryValidator : AbstractValidator<DiscoverChannelsQuery>
{
    public DiscoverChannelsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.Search).MaximumLength(200).When(x => x.Search is not null);
    }
}

public sealed class DiscoverChannelsQueryHandler(
    ChatDbContext db,
    ICurrentUser currentUser)
    : IQueryHandler<DiscoverChannelsQuery, ReadOnlyCollection<ChannelDto>>
{
    public async ValueTask<ReadOnlyCollection<ChannelDto>> Handle(DiscoverChannelsQuery q, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(q);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        int page = Math.Max(1, q.Page);
        int pageSize = Math.Clamp(q.PageSize, 1, 200);

        var query = db.Channels.AsNoTracking()
            .Where(c => c.Type == ChannelType.Channel
                     && !c.IsPrivate
                     && !c.Members.Any(m => m.UserId == currentUserId));

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();
            query = query.Where(c =>
                EF.Functions.ILike(c.Name!, $"%{term}%")
                || EF.Functions.ILike(c.Slug!, $"%{term}%"));
        }

        var channels = await query
            .OrderByDescending(c => c.LastMessageAtUtc ?? c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return channels.Select(c => c.ToDto()).ToList().AsReadOnly();
    }
}