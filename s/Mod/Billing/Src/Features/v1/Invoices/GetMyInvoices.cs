using FSH.Framework.Shared.Persistence;
using FSH.Mod.Billing.Data;
using FSH.Mod.Billing.Spec;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Billing.Features.v1.Invoices;

public static class GetMyInvoicesEndpoint
{
    internal static RouteHandlerBuilder MapGetMyInvoicesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/invoices/me",
                (InvoiceStatus? status, int? periodYear, int? periodMonth,
                 int pageNumber, int pageSize, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetMyInvoicesQuery(
                        status,
                        periodYear,
                        periodMonth,
                        pageNumber <= 0 ? 1 : pageNumber,
                        pageSize <= 0 ? 20 : Math.Min(pageSize, 100)), ct))
            .WithName("GetMyInvoices")
            .WithSummary("List invoices for the application");
    }
}

public sealed class GetMyInvoicesQueryHandler(
    BillingDbContext dbContext)
    : IQueryHandler<GetMyInvoicesQuery, PagedResponse<InvoiceDto>>
{
    public async ValueTask<PagedResponse<InvoiceDto>> Handle(GetMyInvoicesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var q = dbContext.Invoices.AsNoTracking()
            .Include(i => i.LineItems).AsQueryable();
        if (query.Status is not null)
        {
            q = q.Where(i => i.Status == query.Status);
        }
        if (query.PeriodYear is not null)
        {
            q = q.Where(i => i.PeriodYear == query.PeriodYear);
        }
        if (query.PeriodMonth is not null)
        {
            q = q.Where(i => i.PeriodMonth == query.PeriodMonth);
        }

        var total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var invoices = await q
            .OrderByDescending(i => i.CreatedAtUtc)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResponse<InvoiceDto>
        {
            Items = invoices.Select(i => i.ToDto()).ToList(),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        };
    }
}