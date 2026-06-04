namespace FSH.Modules.Identity.Contracts.Services;

/// <summary>
/// Service for managing user referral relationships.
/// </summary>
public interface IReferralService
{
    /// <summary>
    /// Gets or creates a referral code for the specified user.
    /// </summary>
    Task<string> GetOrCreateReferralCodeAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the referral code for a user.
    /// </summary>
    Task<string?> GetReferralCodeAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Records a referral when a referred user registers.
    /// </summary>
    Task RecordReferralAsync(string referralCode, string referredUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Records a referral visit for a logged-in user. Returns the referrer name if found.
    /// </summary>
    Task<string?> RecordReferralVisitAsync(string referralCode, string visitedUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all users referred by the specified user.
    /// </summary>
    Task<List<ReferralDto>> GetReferredUsersAsync(string userId, CancellationToken cancellationToken = default);
}

public record ReferralDto(string ReferralCode, string? ReferredUserId, string? ReferredUserName, string? ReferredUserEmail, DateTime CreatedAt, DateTime? UsedAt);