using FSH.Modules.Billing.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Billing.Contracts.v1.Plans;

public sealed record GetPlansQuery(bool IncludeInactive = false) : IQuery<IReadOnlyList<BillingPlanDto>>;