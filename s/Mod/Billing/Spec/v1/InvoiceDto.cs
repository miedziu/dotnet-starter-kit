namespace FSH.Mod.Billing.Spec.v1;

public sealed record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    int PeriodYear,
    int PeriodMonth,
    string Currency,
    decimal SubtotalAmount,
    InvoiceStatus Status,
    DateTime CreatedAtUtc,
    DateTime? IssuedAtUtc,
    DateTime? DueAtUtc,
    DateTime? PaidAtUtc,
    DateTime? VoidedAtUtc,
    string? Notes,
    IReadOnlyList<InvoiceLineItemDto> LineItems,
    InvoicePurpose Purpose,
    DateTime? PeriodStartUtc,
    DateTime? PeriodEndUtc);