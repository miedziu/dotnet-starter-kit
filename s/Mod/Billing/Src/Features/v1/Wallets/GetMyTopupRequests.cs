using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using FSH.Mod.Billing.Data;
using FSH.Mod.Billing.Mappings;
using FSH.Mod.Billing.Spec;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Billing.Features.v1.Wallets;

public static class GetMyTopupRequestsEndpoint
{
    internal static RouteHandlerBuilder MapGetMyTopupRequestsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/wallet/topup-requests/me",
                (TopupRequestStatus? status, int pageNumber, int pageSize, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetMyTopupRequestsQuery(
                        status,
                        pageNumber <= 0 ? 1 : pageNumber,
                        pageSize <= 0 ? 20 : Math.Min(pageSize, 100)), ct))
            .WithName("GetMyTopupRequests")
            .WithSummary("List top-up requests for the current tenant")
            .RequirePermission(BillingPermissions.View);
    }
}

public sealed class GetMyTopupRequestsQueryValidator : AbstractValidator<GetMyTopupRequestsQuery>
{
    public GetMyTopupRequestsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetMyTopupRequestsQueryHandler(
    BillingDbContext dbContext)
    : IQueryHandler<GetMyTopupRequestsQuery, PagedResponse<TopupRequestDto>>
{
    public async ValueTask<PagedResponse<TopupRequestDto>> Handle(GetMyTopupRequestsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var q = dbContext.TopupRequests.AsNoTracking();

        if (query.Status is not null)
        {
            q = q.Where(r => r.Status == query.Status);
        }

        var total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResponse<TopupRequestDto>
        {
            Items = items.Select(r => r.ToDto()).ToList(),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        };
    }
}