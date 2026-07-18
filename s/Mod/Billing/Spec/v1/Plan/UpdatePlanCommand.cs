using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Plan;

public sealed record UpdatePlanCommand(
    Guid PlanId,
    string Name,
    decimal MonthlyBasePrice,
    PlanInterval Interval = PlanInterval.Monthly,
    decimal? AnnualPrice = null) : ICommand<Guid>;