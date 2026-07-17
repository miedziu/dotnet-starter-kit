using Mediator;

namespace FSH.Modules.Billing.Contracts.v1.Plans;

public sealed record CreatePlanCommand(
    string Key,
    string Name,
    string Currency,
    decimal MonthlyBasePrice,
    PlanInterval Interval = PlanInterval.Monthly,
    decimal? AnnualPrice = null) : ICommand<Guid>;