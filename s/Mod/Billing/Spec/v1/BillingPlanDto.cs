namespace FSH.Modules.Billing.Contracts.v1.Dtos;

public sealed record BillingPlanDto(
    Guid Id,
    string Key,
    string Name,
    string Currency,
    decimal MonthlyBasePrice,
    bool IsActive,
    PlanInterval Interval,
    decimal? AnnualPrice);