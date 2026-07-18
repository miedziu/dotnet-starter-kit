using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Subscription;

/// <summary>
/// Returns the current active subscription.
/// </summary>
public sealed record GetSubscriptionQuery() : IQuery<SubscriptionDto?>;