using FSH.Mod.File.Spec;

namespace FSH.Mod.File.Authorization;

/// <summary>
/// Default policy used for the built-in <c>MyFiles</c> and <c>User</c> owner types.
/// - Attach: any authenticated user (currentUserId is non-empty).
/// - Read: Public files visible to anyone; Private files only to the uploader.
/// - Delete: only the uploader.
///
/// Scoping is handled by the framework's BaseDbContext, not here.
/// Owning modules with different rules (e.g. Ticket — participants only) register their own
/// <see cref="IFileAccessPolicy"/> implementation that supersedes this one.
/// </summary>
internal sealed class DefaultUploaderOnlyPolicy : IFileAccessPolicy
{
    public DefaultUploaderOnlyPolicy(string ownerType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerType);
        OwnerType = ownerType;
    }

    public string OwnerType { get; }

    public Task<bool> CanAttachAsync(Guid? ownerId, string currentUserId, CancellationToken ct)
        => Task.FromResult(!string.IsNullOrEmpty(currentUserId));

    public Task<bool> CanReadAsync(FileAccessContext context, string currentUserId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (string.IsNullOrEmpty(currentUserId)) return Task.FromResult(false);

        // Visibility 0 = Public, visible to anyone.
        if (context.Visibility == 0) return Task.FromResult(true);
        return Task.FromResult(IsUploader(context, currentUserId));
    }

    public Task<bool> CanDeleteAsync(FileAccessContext context, string currentUserId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (string.IsNullOrEmpty(currentUserId)) return Task.FromResult(false);
        return Task.FromResult(IsUploader(context, currentUserId));
    }

    private static bool IsUploader(FileAccessContext context, string currentUserId)
        => string.Equals(currentUserId, context.CreatedByUserId, StringComparison.Ordinal);
}