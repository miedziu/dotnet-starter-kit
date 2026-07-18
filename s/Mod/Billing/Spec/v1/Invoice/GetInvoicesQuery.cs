using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Invoice;

/// <summary>
/// Lists invoices with optional filters.
/// </summary>
public sealed record GetInvoicesQuery(
    InvoiceStatus? Status = null,
    int? PeriodYear = null,
    int? PeriodMonth = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResponse<InvoiceDto>>;