using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Invoice;

public sealed record VoidInvoiceCommand(Guid InvoiceId, string? Reason = null) : ICommand<Guid>;