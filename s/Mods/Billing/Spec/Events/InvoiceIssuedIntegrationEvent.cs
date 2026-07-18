using FSH.Framework.Eventing.Abstractions;

namespace FSH.Mods.Billing.Spec.Events;

/// <summary>
/// Raised when an invoice transitions to Issued and becomes a real bill (e.g. the subscription invoice
/// generated on subscription create/renew). Consumers notify the application that an invoice is due.
/// </summary>
public sealed record InvoiceIssuedIntegrationEvent(
    Guid Id,
    DateTime OccurredAt,
    string CorrelationId,
    string Source,
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Amount,
    string Currency,
    DateTime? DueAtUtc,
    int PeriodYear,
    int PeriodMonth)
    : IIntegrationEvent;