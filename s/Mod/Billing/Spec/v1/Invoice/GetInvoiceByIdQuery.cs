using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Invoice;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<InvoiceDto>;