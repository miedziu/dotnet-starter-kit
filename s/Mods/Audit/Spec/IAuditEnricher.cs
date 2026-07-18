namespace FSH.Mods.Audit.Spec;

/// <summary>
/// Hook to augment events before they are published (e.g., add user/trace, normalize fields, enforce caps).
/// </summary>
public interface IAuditEnricher
{
    /// <summary>Mutate/augment the event instance prior to serialization/publish.</summary>
    void Enrich(IAuditEvent auditEvent);
}