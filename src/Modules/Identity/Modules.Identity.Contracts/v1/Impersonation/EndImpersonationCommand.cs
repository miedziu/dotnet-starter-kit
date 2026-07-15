using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Impersonation;

public sealed record EndImpersonationCommand() : ICommand<TokenResponse>;