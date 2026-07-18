using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Invoice;

public sealed record IssueInvoiceCommand(Guid InvoiceId, DateTime? DueAtUtc = null) : ICommand<Guid>;