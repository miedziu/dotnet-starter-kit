using FSH.Framework.Shared.Storage;

namespace FSH.Mod.Identity.Spec.Services;

/// <summary>
/// Service for user profile operations.
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    Task<UserDto> GetAsync(string userId, CancellationToken ct);

    /// <summary>
    /// Gets all users.
    /// </summary>
    Task<List<UserDto>> GetListAsync(CancellationToken ct);

    /// <summary>
    /// Gets the total user count.
    /// </summary>
    Task<int> GetCountAsync(CancellationToken ct);

    /// <summary>
    /// Updates a user's profile.
    /// </summary>
    Task UpdateAsync(string userId, string firstName, string lastName, string phoneNumber, FileUploadRequest image, bool deleteCurrentImage, CancellationToken ct = default);

    /// <summary>
    /// Sets the profile image URL directly (no upload). Used by the presigned-upload flow:
    /// the client uploads via the Files module, then calls this with the resulting durable
    /// <c>publicUrl</c>. Passing <c>null</c> clears the image.
    /// </summary>
    Task SetImageUrlAsync(string userId, string? imageUrl, CancellationToken ct);

    /// <summary>
    /// Checks if a user exists with the given email.
    /// </summary>
    Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user exists with the given username.
    /// </summary>
    Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user exists with the given phone number.
    /// </summary>
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets the internal integer identifier (IntId) for a user by their Guid Id.
    /// Throws NotFoundException if the user is not found.
    /// </summary>
    Task<int> GetIntIdAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Gets the Guid Id for a user by their internal integer identifier (IntId).
    /// Throws NotFoundException if the user is not found.
    /// </summary>
    Task<Guid> GetGuidAsync(int intId, CancellationToken ct = default);
}