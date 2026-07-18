namespace FSH.Mod.Audit.Spec;

public sealed record EntityChangeEventPayload(
    string DbContext,
    string? Schema,
    string Table,
    string EntityName,
    string Key,                          // unified string key (e.g., "Id:42")
    EntityOperation Operation,
    IReadOnlyList<PropertyChange> Changes,
    string? TransactionId
);