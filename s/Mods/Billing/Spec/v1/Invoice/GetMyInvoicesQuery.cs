using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Invoice;

public sealed record GetMyInvoicesQuery(
    InvoiceStatus? Status = null,
    int? PeriodYear = null,
    int? PeriodMonth = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResponse<InvoiceDto>>;