namespace FSH.Mods.Audit.Spec;

/// <summary>
/// Deterministic JSON serialization for payloads (camelCase, enum-as-string, stable output).
/// </summary>
public interface IAuditSerializer
{
    string SerializePayload(object payload);
}