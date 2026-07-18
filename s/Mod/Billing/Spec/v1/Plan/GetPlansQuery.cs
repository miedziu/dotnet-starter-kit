using Mediator;

namespace FSH.Mod.Billing.Spec.v1.Plan;

public sealed record GetPlansQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<BillingPlanDto>>;