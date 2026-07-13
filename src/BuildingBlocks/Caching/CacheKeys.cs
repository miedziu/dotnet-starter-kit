namespace FSH.Framework.Caching;

/// <summary>
/// Cache key conventions and tag constants used across the FullStackHero starter kit.
/// Tags enable bulk invalidation via
/// <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache.RemoveByTagAsync(string, System.Threading.CancellationToken)"/>.
/// </summary>
public static class CacheKeys
{
    /// <summary>Well-known tag values for bulk invalidation.</summary>
    public static class Tags
    {
        /// <summary>Tag applied to every permission entry.</summary>
        public const string Permissions = "permissions";

        /// <summary>Tag applied to every theme entry.</summary>
        public const string Themes = "themes";

        /// <summary>Tag applied to every idempotency replay entry.</summary>
        public const string Idempotency = "idempotency";

        /// <summary>Per-user tag — invalidates all entries scoped to a user.</summary>
        public static string User(string userId) => $"user:{userId}";
    }

    /// <summary>Key for the permission list of a given user.</summary>
    public static string UserPermissions(string userId) => $"perm:u:{userId}";

    /// <summary>Key for a theme.</summary>
    public static string TenantTheme(string themeId) => $"theme:t:{themeId}";

    /// <summary>Key for the system-wide default theme.</summary>
    public const string DefaultTheme = "theme:default";

    /// <summary>Key for an idempotency replay entry, scoped by user.</summary>
    public static string IdempotencyEntry(string userId, string key) => $"idem:u:{userId}:{key}";

    /// <summary>
    /// Key for the impersonation-grant revocation marker, indexed by JWT id.
    /// Read on every authenticated request that carries an act_sub claim.
    /// </summary>
    public static string ImpersonationGrantStatus(string jti) => $"impgrant:{jti}";
}