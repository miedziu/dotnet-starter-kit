using Mediator;
using System.Text.Json.Serialization;

namespace FSH.Mod.Identity.Spec.v1.User;

public class RegisterUserCommand : ICommand<RegisterUserResponse>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
    public string? PhoneNumber { get; set; }

    [JsonIgnore]
    public string? Origin { get; set; }

    public string[]? ReferralUsernames { get; set; }
}