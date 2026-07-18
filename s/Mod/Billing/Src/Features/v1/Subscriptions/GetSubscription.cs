using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Billing.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Billing.Features.v1.Subscriptions;

public static class GetSubscriptionEndpoint
{
    internal static RouteHandlerBuilder MapGetSubscriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/subscriptions",
                (IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetSubscriptionQuery(), ct))
            .WithName("GetSubscription")
            .WithSummary("Get the active subscription")
            .RequirePermission(BillingPermissions.View);
    }

    internal static RouteHandlerBuilder MapGetMySubscriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/subscriptions/me",
                (IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetSubscriptionQuery(), ct))
            .WithName("GetMySubscription")
            .WithSummary("Get the active subscription");
    }
}

public sealed class GetSubscriptionQueryHandler(
    BillingDbContext dbContext)
    : IQueryHandler<GetSubscriptionQuery, SubscriptionDto?>
{
    public async ValueTask<SubscriptionDto?> Handle(GetSubscriptionQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var sub = await (from s in dbContext.Subscriptions.AsNoTracking()
                         join p in dbContext.Plans.AsNoTracking() on s.PlanId equals p.Id
                         where s.Status == Contracts.SubscriptionStatus.Active
                         select new SubscriptionDto(s.Id, s.PlanId, p.Key, s.StartUtc, s.EndUtc, s.Status))
                         .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return sub;
    }
}