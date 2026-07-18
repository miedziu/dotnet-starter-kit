using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Billing.Data;
using FSH.Mods.Billing.Spec;
using FSH.Mods.Billing.Spec.v1;
using FSH.Mods.Billing.Spec.v1.Invoice;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Billing.Features.v1.Invoices;

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