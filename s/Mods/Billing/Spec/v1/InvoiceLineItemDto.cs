namespace FSH.Mods.Billing.Spec.v1;

public sealed record InvoiceLineItemDto(
    Guid Id,
    InvoiceLineItemKind Kind,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Amount);