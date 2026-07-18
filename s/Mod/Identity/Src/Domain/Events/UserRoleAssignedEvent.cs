using FSH.Framework.Core.Domain;

namespace FSH.Mod.Identity.Domain.Events;

/// <summary>Raised when roles are assigned to a user.</summary>
public sealed record UserRoleAssignedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    string UserId,
    IReadOnlyList<string> AssignedRoles,
    string? CorrelationId = null
) : DomainEvent(EventId, OccurredAt, CorrelationId)
{
    public static UserRoleAssignedEvent Create(string userId, IEnumerable<string> assignedRoles, string? correlationId = null)
        => new(Guid.NewGuid(), DateTimeOffset.UtcNow, userId, assignedRoles.ToList().AsReadOnly(), correlationId);
}