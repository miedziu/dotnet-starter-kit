using FSH.Modules.Billing.Contracts;
using FSH.Modules.Billing.Data;
using FSH.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Billing.Services;

/// <summary>
/// Manages the global subscription state for the billing module.
/// Billing is not tenant-scoped: there is a single subscription ledger
/// shared across the system. This service provides the operations needed
/// by tenant lifecycle events (subscribe/renew).
/// </summary>
public static class TenantSubscriptionMaintenance
{
    /// <summary>
    /// Replaces the active subscription with a new one (plan change or new tenant).
    /// Any existing active subscription is cancelled with its EndUtc set to the new start.
    /// </summary>
    public static async Task ReplaceActiveSubscriptionAsync(
        BillingDbContext db,
        Guid planId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        var existing = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.Status == SubscriptionStatus.Active, ct)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            existing.Cancel(startUtc);
        }

        var subscription = Subscription.Create(planId, startUtc, endUtc);
        db.Subscriptions.Add(subscription);
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Extends the active subscription's term for a same-plan renewal.
    /// Idempotent: only moves the end forward.
    /// </summary>
    public static async Task ExtendActiveSubscriptionAsync(
        BillingDbContext db,
        DateTime endUtc,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        var existing = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.Status == SubscriptionStatus.Active, ct)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            existing.Extend(endUtc);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }
}