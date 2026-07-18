using FSH.Framework.Core.Domain;
using FSH.Mods.Identity.Domain.Events;
using Microsoft.AspNetCore.Identity;

namespace FSH.Mods.Identity.Domain;

public class FshUser : IdentityUser, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public int IntId { get; init; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Uri? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

    public string? ObjectId { get; set; }

    // Address fields
    public short? DistrictId { get; set; }
    public short? CommuneId { get; set; }

    public DateTime CreatedAt { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;

    /// <summary>Timestamp when the user last changed their password</summary>
    public DateTime LastPasswordChangeDate { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;

    // Navigation property for password history
    public virtual ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();

    // Navigation collection for users this user has referred (via Referrals table)
    public virtual ICollection<Referral> Referrals { get; set; } = new List<Referral>();

    // IHasDomainEvents implementation
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Records UserRegisteredEvent. Call after user creation.</summary>
    public void RecordRegistered()
    {
        AddDomainEvent(UserRegisteredEvent.Create(
            userId: Id,
            email: Email ?? string.Empty,
            firstName: FirstName,
            lastName: LastName));
    }

    /// <summary>Records PasswordChangedEvent. Call after password change.</summary>
    public void RecordPasswordChanged(bool wasReset = false)
    {
        AddDomainEvent(PasswordChangedEvent.Create(
            userId: Id,
            wasReset: wasReset));
    }

    /// <summary>Sets user to active and records UserActivatedEvent.</summary>
    public void Activate(string? activatedBy = null)
    {
        if (IsActive) return;
        IsActive = true;
        AddDomainEvent(UserActivatedEvent.Create(
            userId: Id,
            activatedBy: activatedBy));
    }

    /// <summary>Sets user to inactive and records UserDeactivatedEvent.</summary>
    public void Deactivate(string? deactivatedBy = null, string? reason = null)
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(UserDeactivatedEvent.Create(
            userId: Id,
            deactivatedBy: deactivatedBy,
            reason: reason));
    }

    /// <summary>Records UserRoleAssignedEvent. Call after roles are assigned.</summary>
    public void RecordRolesAssigned(IEnumerable<string> assignedRoles)
    {
        var rolesList = assignedRoles.ToList();
        if (rolesList.Count == 0) return;
        AddDomainEvent(UserRoleAssignedEvent.Create(
            userId: Id,
            assignedRoles: rolesList));
    }
}