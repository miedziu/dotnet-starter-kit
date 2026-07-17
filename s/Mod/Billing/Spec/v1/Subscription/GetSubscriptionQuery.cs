using FSH.Modules.Billing.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Billing.Contracts.v1.Subscriptions;

/// <summary>
/// Returns the current active subscription.
/// </summary>
public sealed record GetSubscriptionQuery() : IQuery<SubscriptionDto?>;