using Microsoft.AspNetCore.Identity;

namespace FSH.Mods.Identity.Domain;

public class FshRoleClaim : IdentityRoleClaim<string>
{
    public string? CreatedBy { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}