using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Plan;

public sealed record CreatePlanCommand(
    string Key,
    string Name,
    string Currency,
    decimal MonthlyBasePrice,
    PlanInterval Interval = PlanInterval.Monthly,
    decimal? AnnualPrice = null) : ICommand<Guid>;