using System.Text.RegularExpressions;

namespace FSH.Mods.File.Services;

/// <summary>
/// Builds canonical storage keys for the File module:
///   files/{ownerType-lower}/{yyyy}/{MM}/{fileAssetId:N}/{sanitized-filename}.
/// </summary>
public static partial class StorageKeyBuilder
{
    [GeneratedRegex(@"[^a-zA-Z0-9_\.-]")]
    private static partial Regex UnsafeChars();

    public static string Build(string ownerType, Guid fileAssetId, string fileName, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerType);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

#pragma warning disable CA1308 // path segments are intentionally lower-case
        var lowerOwner = ownerType.ToLowerInvariant();
#pragma warning restore CA1308
        var safe = Sanitize(fileName);
        return $"files/{lowerOwner}/{now:yyyy}/{now:MM}/{fileAssetId:N}/{safe}";
    }

    public static string Sanitize(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        return UnsafeChars().Replace(fileName, "_");
    }
}