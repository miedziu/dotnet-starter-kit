using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Impersonation;

public sealed record EndImpersonationCommand() : ICommand<TokenResponse>;