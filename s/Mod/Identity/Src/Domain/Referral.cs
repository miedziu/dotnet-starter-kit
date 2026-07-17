namespace FSH.Modules.Identity.Domain;

/// <summary>
/// Represents a referral conversion - when a user registers via another user's referral link.
/// Uses composite key (ReferrerUserId, NewReferredUserId) to prevent duplicate referrals.
/// Both columns use int (IntId) for internal FK relationships.
/// </summary>
public class Referral
{
    public int ReferrerUserId { get; private set; }

    public int NewReferredUserId { get; private set; }

    // Navigation properties
    public virtual FshUser ReferrerUser { get; private set; } = default!;
    public virtual FshUser NewReferredUser { get; private set; } = default!;

    private Referral() { } // EF Core

    public static Referral Create(int referrerUserId, int newReferredUserId)
    {
        return new Referral
        {
            ReferrerUserId = referrerUserId,
            NewReferredUserId = newReferredUserId,
        };
    }
}