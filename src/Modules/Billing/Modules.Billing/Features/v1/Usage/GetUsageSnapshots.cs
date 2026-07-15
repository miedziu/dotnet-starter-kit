using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Billing.Contracts.Authorization;
using FSH.Modules.Billing.Contracts.v1.Dtos;
using FSH.Modules.Billing.Contracts.v1.Usage;
using FSH.Modules.Billing.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Billing.Features.v1.Usage;

public static class GetUsageSnapshotsEndpoint
{
    internal static RouteHandlerBuilder MapGetUsageSnapshotsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/usage",
                (int? periodYear, int? periodMonth, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetUsageSnapshotsQuery(periodYear, periodMonth), ct))
            .WithName("GetUsageSnapshots")
            .WithSummary("List captured usage snapshots")
            .RequirePermission(BillingPermissions.View);
    }
}

public sealed class GetUsageSnapshotsQueryHandler(
    BillingDbContext dbContext)
    : IQueryHandler<GetUsageSnapshotsQuery, IReadOnlyList<UsageSnapshotDto>>
{
    public async ValueTask<IReadOnlyList<UsageSnapshotDto>> Handle(GetUsageSnapshotsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var q = dbContext.UsageSnapshots.AsNoTracking();
        if (query.PeriodYear is not null)
        {
            q = q.Where(s => s.PeriodYear == query.PeriodYear);
        }
        if (query.PeriodMonth is not null)
        {
            q = q.Where(s => s.PeriodMonth == query.PeriodMonth);
        }

        var snaps = await q
            .OrderByDescending(s => s.PeriodYear).ThenByDescending(s => s.PeriodMonth)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return snaps
            .Select(s => new UsageSnapshotDto(s.Id, s.PeriodYear, s.PeriodMonth, s.UsedUnits, s.LimitUnits, s.Overage, s.CapturedAtUtc))
            .ToList();
    }
}