using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Billing.Data;
using FSH.Mods.Billing.Spec;
using FSH.Mods.Billing.Spec.v1;
using FSH.Mods.Billing.Spec.v1.Plan;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Billing.Features.v1.Plans;

public static class GetPlansEndpoint
{
    internal static RouteHandlerBuilder MapGetPlansEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/plans",
                (bool includeInactive, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetPlansQuery(includeInactive), ct))
            .WithName("GetBillingPlans")
            .WithSummary("List billing plans")
            .RequirePermission(BillingPermissions.View);
    }
}

public sealed class GetPlansQueryHandler(BillingDbContext dbContext)
    : IQueryHandler<GetPlansQuery, IReadOnlyList<BillingPlanDto>>
{
    public async ValueTask<IReadOnlyList<BillingPlanDto>> Handle(GetPlansQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var plansQuery = dbContext.Plans.AsNoTracking();
        if (!query.IncludeInactive)
        {
            plansQuery = plansQuery.Where(p => p.IsActive);
        }

        var plans = await plansQuery.OrderBy(p => p.Key).ToListAsync(cancellationToken).ConfigureAwait(false);
        return plans
            .Select(p => new BillingPlanDto(p.Id, p.Key, p.Name, p.Currency, p.MonthlyBasePrice.Amount, p.IsActive, p.Interval, p.AnnualPrice?.Amount))
            .ToList();
    }
}