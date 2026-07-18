namespace FSH.Mod.Identity.Domain;

public class PasswordHistory
{
    public int Id { get; init; }
    public int UserId { get; private set; }
    public string PasswordHash { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    // Navigation property (init for EF Core materialization)
    public virtual FshUser? User { get; init; }

    private PasswordHistory() { } // EF Core

    public static PasswordHistory Create(int userId, string passwordHash)
    {
        return new PasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = TimeProvider.System.GetUtcNow().UtcDateTime
        };
    }
}