namespace FSH.Mod.Identity.Spec.Services;

public interface IPasswordHistoryService
{
    /// <summary>Check if the new password matches any recent passwords in history.</summary>
    Task<bool> IsPasswordInHistoryAsync(string userId, string newPassword, CancellationToken ct = default);

    /// <summary>Save the current password hash to history after a password change.</summary>
    Task SavePasswordHistoryAsync(string userId, CancellationToken ct = default);

    /// <summary>Remove old password history entries beyond the configured retention count.</summary>
    Task CleanupOldPasswordHistoryAsync(string userId, CancellationToken ct = default);
}