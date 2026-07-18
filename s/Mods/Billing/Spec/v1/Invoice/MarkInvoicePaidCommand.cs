using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Invoice;

public sealed record MarkInvoicePaidCommand(Guid InvoiceId) : ICommand<Guid>;