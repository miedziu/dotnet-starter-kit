using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Subscription;

/// <summary>
/// Command to assign to a plan, starting now. If there is an existing active
/// subscription it will be cancelled at this moment and replaced.
/// </summary>
public sealed record AssignSubscriptionCommand(string PlanKey) : ICommand<Guid>;