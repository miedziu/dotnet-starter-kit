using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FSH.Modules.Identity.Services;

internal sealed class ReferralService(
    IdentityDbContext db) : IReferralService
{
    private static readonly char[] AllowedChars = "abcdefghjkmnpqrstuvwxyz23456789".ToCharArray(); // No confusing chars: i,l,o,0,1
    
    public async Task<string> GetOrCreateReferralCodeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var existingReferral = await db.Referrals
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReferrerUserId == userId, cancellationToken);
        
        if (existingReferral != null)
        {
            return existingReferral.ReferralCode;
        }
        
        var code = GenerateUniqueReferralCode();
        
        var referral = Domain.Referral.Create(code, userId);
        db.Referrals.Add(referral);
        await db.SaveChangesAsync(cancellationToken);
        
        return code;
    }
    
    public async Task<string?> GetReferralCodeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var referral = await db.Referrals
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReferrerUserId == userId, cancellationToken);
        
        return referral?.ReferralCode;
    }
    
    public async Task RecordReferralAsync(string referralCode, string referredUserId, CancellationToken cancellationToken = default)
    {
        var referral = await db.Referrals
            .FirstOrDefaultAsync(r => r.ReferralCode == referralCode && r.ReferredUserId == null, cancellationToken);
        
        if (referral == null)
        {
            // Code doesn't exist or already used - silently ignore to not leak information
            return;
        }
        
        referral.MarkUsed(referredUserId);
        await db.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<string?> RecordReferralVisitAsync(string referralCode, string visitedUserId, CancellationToken cancellationToken = default)
    {
        var referral = await db.Referrals
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReferralCode == referralCode, cancellationToken);
        
        if (referral == null)
        {
            return null;
        }
        
        // Check if this user has already been recorded for this referral
        var alreadyRecorded = await db.Referrals
            .AsNoTracking()
            .AnyAsync(r => r.ReferralCode == referralCode && r.ReferredUserId == visitedUserId, cancellationToken);
        
        if (!alreadyRecorded)
        {
            // Create a new referral record linking referrer to visited user
            var visitRecord = Domain.Referral.Create($"{referralCode}-visit-{Guid.NewGuid():N}"[..16], referral.ReferrerUserId);
            visitRecord.MarkUsed(visitedUserId);
            db.Referrals.Add(visitRecord);
            await db.SaveChangesAsync(cancellationToken);
        }
        
        // Get referrer's name
        var referrer = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == referral.ReferrerUserId, cancellationToken);
        
        return referrer?.FirstName ?? referrer?.UserName ?? "A user";
    }
    
    public async Task<List<ReferralDto>> GetReferredUsersAsync(string userId, CancellationToken cancellationToken = default)
    {
        var referrals = await db.Referrals
            .AsNoTracking()
            .Where(r => r.ReferrerUserId == userId)
            .Select(r => new ReferralDto(
                r.ReferralCode,
                r.ReferredUserId,
                r.ReferredUser != null ? r.ReferredUser.FirstName : null,
                r.ReferredUser != null ? r.ReferredUser.Email : null,
                r.CreatedAt,
                r.UsedAt))
            .ToListAsync(cancellationToken);
        
        return referrals;
    }
    
    private static string GenerateUniqueReferralCode()
    {
        // Generate 10-character alphanumeric code (excluding confusing characters)
        // Using cryptographically secure random for security
        var bytes = RandomNumberGenerator.GetBytes(10);
        var code = string.Create(10, bytes, (span, data) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = AllowedChars[data[i] % AllowedChars.Length];
            }
        });
        return code;
    }
}
