namespace FSH.Mod.Audit.Spec;

/// <summary>
/// Enricher that can return a modified event (e.g., fill missing fields, mask payload).
/// </summary>
public interface IAuditMutatingEnricher
{
    AuditEnvelope Enrich(AuditEnvelope envelope);
}