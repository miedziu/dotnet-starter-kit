using FSH.Framework.Shared.Persistence;
using FSH.Modules.Billing.Contracts.Dtos;
using FSH.Modules.Billing.Contracts.v1.Wallets;
using FSH.Modules.Billing.Data;
using FSH.Modules.Billing.Mappings;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Billing.Features.v1.Wallets.GetTopupRequests;

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