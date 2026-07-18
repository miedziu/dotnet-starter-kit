using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Invoice;

/// <summary>
/// Admin-triggered invoice generation for a specific billing period.
/// Idempotent: re-running for a period that already has invoices skips those.
/// </summary>
public sealed record GenerateInvoicesCommand(int PeriodYear, int PeriodMonth) : ICommand<int>;