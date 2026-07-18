using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Wallet;

public sealed record GetMyTopupRequestsQuery(
    TopupRequestStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResponse<TopupRequestDto>>;