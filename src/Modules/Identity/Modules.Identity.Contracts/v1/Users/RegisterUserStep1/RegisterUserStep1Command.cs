using Mediator;
using System.Text.Json.Serialization;

namespace FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep1;

public class RegisterUserStep1Command : ICommand<RegisterUserStep1Response>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;

    [JsonIgnore]
    public string? Origin { get; set; }

    public string[]? ReferralUsernames { get; set; }
}