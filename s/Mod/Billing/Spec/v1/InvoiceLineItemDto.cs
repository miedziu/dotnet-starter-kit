namespace FSH.Modules.Billing.Contracts.v1.Dtos;

public sealed record InvoiceLineItemDto(
    Guid Id,
    InvoiceLineItemKind Kind,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Amount);