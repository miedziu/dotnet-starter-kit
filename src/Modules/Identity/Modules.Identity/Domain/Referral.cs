namespace FSH.Modules.Identity.Domain;

/// <summary>
/// Represents a referral conversion - when a user registers via another user's referral link.
/// Uses composite key (ReferrerUserId, NewReferredUserId) to prevent duplicate referrals.
///
/// </summary>
public class Referral
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