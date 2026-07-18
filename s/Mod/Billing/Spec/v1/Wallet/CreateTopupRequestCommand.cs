using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Wallet;

public sealed record CreateTopupRequestCommand(decimal Amount, string? Note) : ICommand<Guid>;