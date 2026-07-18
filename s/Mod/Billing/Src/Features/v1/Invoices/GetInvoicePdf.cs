using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Billing.Data;
using FSH.Mod.Billing.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Billing.Features.v1.Invoices;

public static class GetInvoicePdfEndpoint
{
    internal static RouteHandlerBuilder MapGetInvoicePdfEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/invoices/{invoiceId:guid}/pdf",
                async (Guid invoiceId, IMediator mediator, CancellationToken ct) =>
                {
                    var result = await mediator.Send(new GetInvoicePdfQuery(invoiceId), ct).ConfigureAwait(false);
                    return Results.File(result.Content, "application/pdf", result.FileName);
                })
            .WithName("GetInvoicePdf")
            .WithSummary("Download an invoice as a PDF")
            // BillingPermissions.View is basic (granted to tenant users), and the handler scopes to the
            // caller's tenant — so this single endpoint serves both operators and tenant self-service.
            .RequirePermission(BillingPermissions.View)
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces(StatusCodes.Status404NotFound);
    }
}

/// <summary>Fetches the caller's invoice and renders it to a PDF. Module-internal (the byte[]
/// result is not a cross-module contract).</summary>
public sealed record GetInvoicePdfQuery(Guid InvoiceId) : IQuery<InvoicePdfResult>;

public sealed record InvoicePdfResult(byte[] Content, string FileName);

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