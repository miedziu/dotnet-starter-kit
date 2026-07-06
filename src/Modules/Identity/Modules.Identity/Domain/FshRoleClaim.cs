using FSH.Framework.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace FSH.Modules.Identity.Domain;

public class FshRoleClaim : IdentityRoleClaim<string>, IGlobalEntity
{
    public string? CreatedBy { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}