using FSH.Framework.Core.Domain;

namespace FSH.Modules.Identity.Domain;

/// <summary>
/// Represents a referral conversion - when a user registers via another user's referral link.
/// Uses composite key (ReferrerUserId, NewReferredUserId) to prevent duplicate referrals.
///
/// Implements <see cref="IGlobalEntity"/> to opt out of tenant isolation. Referrals are
/// created within a tenant context but the relationship between referrer and referred
/// user is inherently tenant-specific. Since both users belong to the same tenant,
/// the TenantId column would be redundant for filtering purposes.
/// </summary>
public class Referral : IGlobalEntity
{
    public string ReferrerUserId { get; private set; } = default!;

    public string NewReferredUserId { get; private set; } = default!;

    // Navigation properties
    public virtual FshUser ReferrerUser { get; private set; } = default!;
    public virtual FshUser NewReferredUser { get; private set; } = default!;

    private Referral() { } // EF Core

    public static Referral Create(string referrerUserId, string newReferredUserId)
    {
        return new Referral
        {
            ReferrerUserId = referrerUserId,
            NewReferredUserId = newReferredUserId,
        };
    }
}