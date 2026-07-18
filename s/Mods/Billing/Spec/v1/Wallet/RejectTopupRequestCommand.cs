using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Wallet;

/// <summary>
/// Operator command — rejects a Pending top-up request. Returns the request id.
/// </summary>
public sealed record RejectTopupRequestCommand(Guid Id, string? Reason) : ICommand<Guid>;