using FSH.Framework.Core.Exceptions;
using FSH.Modules.Billing.Contracts.Dtos;
using FSH.Modules.Billing.Data;
using FSH.Modules.Billing.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Billing.Features.v1.Invoices.GetInvoicePdf;

public sealed class GetInvoicePdfQueryHandler(
    BillingDbContext dbContext,
    IInvoicePdfRenderer renderer)
    : IQueryHandler<GetInvoicePdfQuery, InvoicePdfResult>
{
    public async ValueTask<InvoicePdfResult> Handle(GetInvoicePdfQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var invoice = await dbContext.Invoices.AsNoTracking()
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {query.InvoiceId} not found.");

        var dto = invoice.ToDto();
        var content = renderer.Render(dto);
        return new InvoicePdfResult(content, $"{dto.InvoiceNumber}.pdf");
    }
}