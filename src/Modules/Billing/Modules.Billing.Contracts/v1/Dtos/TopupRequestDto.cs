namespace FSH.Modules.Billing.Contracts.v1.Dtos;

public sealed record TopupRequestDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string? Note,
    string Status,
    Guid? InvoiceId,
    string? RequestedBy,
    string? DecisionNote,
    DateTime CreatedAtUtc,
    DateTime? DecidedAtUtc,
    DateTime? CompletedAtUtc);