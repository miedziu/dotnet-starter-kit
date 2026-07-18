using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Wallet;

public sealed record CreateTopupRequestCommand(decimal Amount, string? Note) : ICommand<Guid>;