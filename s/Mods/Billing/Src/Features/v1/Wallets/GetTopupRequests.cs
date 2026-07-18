using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using FSH.Mods.Billing.Data;
using FSH.Mods.Billing.Mappings;
using FSH.Mods.Billing.Spec;
using FSH.Mods.Billing.Spec.v1;
using FSH.Mods.Billing.Spec.v1.Wallet;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Billing.Features.v1.Wallets;

public static class GetTopupRequestsEndpoint
{
    internal static RouteHandlerBuilder MapGetTopupRequestsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/wallet/topup-requests",
                (string? tenantId, TopupRequestStatus? status, int pageNumber, int pageSize,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetTopupRequestsQuery(
                        tenantId,
                        status,
                        pageNumber <= 0 ? 1 : pageNumber,
                        pageSize <= 0 ? 20 : Math.Min(pageSize, 100)), ct))
            .WithName("GetTopupRequests")
            .WithSummary("List top-up requests across all tenants (operator admin)")
            .RequirePermission(BillingPermissions.View);
    }
}

public sealed class GetTopupRequestsQueryValidator : AbstractValidator<GetTopupRequestsQuery>
{
    public GetTopupRequestsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetTopupRequestsQueryHandler(
    BillingDbContext dbContext)
    : IQueryHandler<GetTopupRequestsQuery, PagedResponse<TopupRequestDto>>
{
    public async ValueTask<PagedResponse<TopupRequestDto>> Handle(GetTopupRequestsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var q = dbContext.TopupRequests.AsNoTracking().AsQueryable();
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