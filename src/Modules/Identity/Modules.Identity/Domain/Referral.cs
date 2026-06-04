using FSH.Framework.Core.Domain;

namespace FSH.Modules.Identity.Domain;

/// <summary>
/// Represents a referral relationship between users. A user can have one referral code
/// that can be shared, and multiple referral records linking them to users they've referred.
/// </summary>
public class Referral : BaseEntity<Guid>
{
    public string ReferralCode { get; private set; } = default!;
    
    public string ReferrerUserId { get; private set; } = default!;
    public string? ReferredUserId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    
    // Navigation properties
    public virtual FshUser ReferrerUser { get; private set; } = default!;
    public virtual FshUser? ReferredUser { get; set; }
    
    private Referral() { } // EF Core
    
    public static Referral Create(string referralCode, string referrerUserId)
    {
        return new Referral
        {
            ReferralCode = referralCode,
            ReferrerUserId = referrerUserId,
            CreatedAt = TimeProvider.System.GetUtcNow().UtcDateTime,
        };
    }
    
    public void MarkUsed(string referredUserId)
    {
        ReferredUserId = referredUserId;
        UsedAt = TimeProvider.System.GetUtcNow().UtcDateTime;
    }
}
