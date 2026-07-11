using FSH.Modules.Billing.Domain;

namespace FSH.Modules.Billing.Services;

/// <summary>
/// Core billing workflow: snapshot usage, price it, issue the invoice, track payment state.
/// Payment processor integration is intentionally out of scope — invoices are marked paid manually.
/// </summary>
public interface IBillingService
{
    /// <summary>
    /// Returns the global wallet, creating one if none exists. Billing is not tenant-scoped: there
    /// is a single wallet ledger for prepaid credit (e.g. WhatsApp) shared across the system.
    /// </summary>
    Task<Wallet> GetOrCreateWalletAsync(string currency, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and issues a Topup-purpose invoice for the pending <see cref="TopupRequest"/>,
    /// fires <c>InvoiceIssuedIntegrationEvent</c>, calls <c>request.MarkInvoiced</c>, and saves —
    /// all in one unit of work.
    /// </summary>
    Task<Invoice> CreateTopupInvoiceAsync(Guid topupRequestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a Draft invoice for the period by snapshotting usage and pricing it against the
    /// active subscription plan. Returns null if there is no active subscription or an invoice
    /// already exists for the period.
    /// </summary>
    Task<Invoice?> GenerateInvoiceForPeriodAsync(
        int periodYear,
        int periodMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the draft invoice for the global active subscription for the given period.
    /// Returns the count of invoices created (an existing invoice for the period is skipped).
    /// </summary>
    Task<int> GenerateInvoicesAsync(
        int periodYear,
        int periodMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and issues a Subscription-purpose invoice for one plan term (the term base fee). Called
    /// when a subscription is created or renews. Returns null for free/zero-price plans (no invoice).
    /// Idempotent: returns the existing invoice if one already exists for the term.
    /// </summary>
    Task<Invoice?> CreateSubscriptionInvoiceAsync(
        Guid planId,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default);

    Task IssueInvoiceAsync(Guid invoiceId, DateTime? dueAtUtc, CancellationToken cancellationToken = default);

    Task MarkInvoicePaidAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    Task VoidInvoiceAsync(Guid invoiceId, string? reason, CancellationToken cancellationToken = default);
}
