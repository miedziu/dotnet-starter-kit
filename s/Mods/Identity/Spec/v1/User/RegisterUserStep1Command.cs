using Mediator;
using System.Text.Json.Serialization;

namespace FSH.Mods.Identity.Spec.v1.User;

public class RegisterUserStep1Command : ICommand<RegisterUserStep1Response>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;

    [JsonIgnore]
    public string? Origin { get; set; }

    public string[]? ReferralUsernames { get; set; }
}