using FSH.Framework.Eventing.Abstractions;
using FSH.Modules.Billing.Data;
using FSH.Modules.Billing.Services;
using FSH.Modules.Multitenancy.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Billing.IntegrationEventHandlers;

/// <summary>
/// Reacts to a tenant being created + subscribed to a plan: starts the active subscription and issues
/// the term's subscription invoice. Invoice creation is idempotent (guarded by invoice number).
/// </summary>
public sealed class TenantSubscribedIntegrationEventHandler(
    BillingDbContext db,
    IBillingService billing,
    ILogger<TenantSubscribedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<TenantSubscribedIntegrationEvent>
{
    public async Task HandleAsync(TenantSubscribedIntegrationEvent @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await TenantSubscriptionMaintenance.ReplaceActiveSubscriptionAsync(
            db, @event.PlanId, @event.PeriodStartUtc, @event.PeriodEndUtc, ct).ConfigureAwait(false);

        await billing.CreateSubscriptionInvoiceAsync(
            @event.PlanId, @event.PeriodStartUtc, @event.PeriodEndUtc, ct).ConfigureAwait(false);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "[Billing] subscribed to plan {PlanKey}; term ends {End:o}",
                @event.PlanKey, @event.PeriodEndUtc);
        }
    }
}