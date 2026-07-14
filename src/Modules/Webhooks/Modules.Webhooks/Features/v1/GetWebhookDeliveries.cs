using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Webhooks.Contracts.Authorization;
using FSH.Modules.Webhooks.Contracts.Dtos;
using FSH.Modules.Webhooks.Contracts.v1.GetWebhookDeliveries;
using FSH.Modules.Webhooks.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Webhooks.Features.v1.Webhooks;

public static class GetWebhookDeliveriesEndpoint
{
    internal static RouteHandlerBuilder MapGetWebhookDeliveriesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/subscriptions/{subscriptionId:guid}/deliveries", async (
            Guid subscriptionId,
            int pageNumber,
            int pageSize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetWebhookDeliveriesQuery(subscriptionId, pageNumber, pageSize), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetWebhookDeliveries")
        .WithSummary("List webhook deliveries for a subscription")
        .RequirePermission(WebhooksPermissions.Subscriptions.View);
    }
}

public sealed class GetWebhookDeliveriesQueryValidator : AbstractValidator<GetWebhookDeliveriesQuery>
{
    public GetWebhookDeliveriesQueryValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetWebhookDeliveriesQueryHandler(
    WebhookDbContext dbContext) : IQueryHandler<GetWebhookDeliveriesQuery, PagedResponse<WebhookDeliveryDto>>
{
    public async ValueTask<PagedResponse<WebhookDeliveryDto>> Handle(
        GetWebhookDeliveriesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var dbQuery = dbContext.Deliveries
            .AsNoTracking()
            .Where(d => d.SubscriptionId == query.SubscriptionId)
            .OrderByDescending(d => d.AttemptedAtUtc);

        var totalCount = await dbQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(d => new WebhookDeliveryDto
            {
                Id = d.Id,
                SubscriptionId = d.SubscriptionId,
                EventType = d.EventType,
                HttpStatusCode = d.HttpStatusCode,
                Success = d.Success,
                AttemptCount = d.AttemptCount,
                AttemptedAtUtc = d.AttemptedAtUtc,
                ErrorMessage = d.ErrorMessage
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<WebhookDeliveryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }
}