namespace FSH.Modules.Billing.Contracts.Dtos;

public sealed record InvoiceLineItemDto(
    Guid Id,
    InvoiceLineItemKind Kind,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Amount);