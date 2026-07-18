using FSH.Mods.Identity.Spec.v1.Token;
using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Impersonation;

public sealed record EndImpersonationCommand() : ICommand<TokenResponse>;