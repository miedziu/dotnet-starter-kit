using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Invoice;

public sealed record IssueInvoiceCommand(Guid InvoiceId, DateTime? DueAtUtc = null) : ICommand<Guid>;