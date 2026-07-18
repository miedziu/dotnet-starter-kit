namespace FSH.Mod.Billing.Spec.v1;

public sealed record SubscriptionDto(
    Guid Id,
    Guid PlanId,
    string PlanKey,
    DateTime StartUtc,
    DateTime? EndUtc,
    SubscriptionStatus Status);