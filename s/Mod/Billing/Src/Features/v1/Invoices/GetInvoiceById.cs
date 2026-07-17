using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Billing.Contracts.Authorization;
using FSH.Modules.Billing.Contracts.v1.Dtos;
using FSH.Modules.Billing.Contracts.v1.Invoices;
using FSH.Modules.Billing.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Billing.Features.v1.Invoices;

public static class GetInvoiceByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetInvoiceByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/invoices/{invoiceId:guid}",
                (Guid invoiceId, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetInvoiceByIdQuery(invoiceId), ct))
            .WithName("GetInvoiceById")
            .WithSummary("Get a single invoice by id")
            .RequirePermission(BillingPermissions.View);
    }
}

public sealed class GetInvoiceByIdQueryHandler(
    BillingDbContext dbContext)
    : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    public async ValueTask<InvoiceDto> Handle(GetInvoiceByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var invoice = await dbContext.Invoices.AsNoTracking()
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {query.InvoiceId} not found.");

        return invoice.ToDto();
    }
}