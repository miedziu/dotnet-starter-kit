namespace FSH.Mods.Billing.Spec.v1;

public sealed record BillingPlanDto(
    Guid Id,
    string Key,
    string Name,
    string Currency,
    decimal MonthlyBasePrice,
    bool IsActive,
    PlanInterval Interval,
    decimal? AnnualPrice);