using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Invoice;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<InvoiceDto>;