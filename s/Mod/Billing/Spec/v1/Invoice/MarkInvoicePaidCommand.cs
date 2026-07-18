using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Invoice;

public sealed record MarkInvoicePaidCommand(Guid InvoiceId) : ICommand<Guid>;