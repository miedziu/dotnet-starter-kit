using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using FSH.Mod.Webhook.Data;
using FSH.Mod.Webhook.Spec.v1.Subscription;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Webhook.Features.v1;

public static class GetWebhookSubscriptionsEndpoint
{
    internal static RouteHandlerBuilder MapGetWebhookSubscriptionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/subscriptions", async (
            int pageNumber,
            int pageSize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetWebhookSubscriptionsQuery(pageNumber, pageSize), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetWebhookSubscriptions")
        .WithSummary("List webhook subscriptions")
        .RequirePermission(WebhookPermissions.Subscriptions.View);
    }
}

public sealed class GetWebhookSubscriptionsQueryValidator : AbstractValidator<GetWebhookSubscriptionsQuery>
{
    public GetWebhookSubscriptionsQueryValidator()
    {
        // PageSize must be >= 1 — a 0 reaches Math.Ceiling(total / (double)PageSize) and would
        // otherwise surface as a 500 instead of a clean 400.
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetWebhookSubscriptionsQueryHandler(
    WebhookDbContext dbContext) : IQueryHandler<GetWebhookSubscriptionsQuery, PagedResponse<WebhookSubscriptionDto>>
{
    public async ValueTask<PagedResponse<WebhookSubscriptionDto>> Handle(
        GetWebhookSubscriptionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var dbQuery = dbContext.Subscriptions
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAtUtc);

        var totalCount = await dbQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(s => new WebhookSubscriptionDto
            {
                Id = s.Id,
                Url = s.Url,
                Events = s.EventsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                IsActive = s.IsActive,
                CreatedAtUtc = s.CreatedAtUtc
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<WebhookSubscriptionDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }
}