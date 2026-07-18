using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mods.Billing.Spec.v1.Wallet;

/// <summary>
/// Operator query — lists top-up requests across all tenants with optional filters.
/// Root callers get a cross-tenant view (optionally narrowed via <paramref name="TenantId"/>);
/// non-root callers are automatically scoped to their own tenant.
/// </summary>
public sealed record GetTopupRequestsQuery(
    string? TenantId = null,
    TopupRequestStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResponse<TopupRequestDto>>;