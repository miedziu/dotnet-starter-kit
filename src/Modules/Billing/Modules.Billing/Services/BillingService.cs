using FSH.Framework.Core.Exceptions;
using FSH.Framework.Eventing.Abstractions;
using FSH.Modules.Billing.Contracts;
using FSH.Modules.Billing.Contracts.Events;
using FSH.Modules.Billing.Data;
using FSH.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Billing.Services;

public sealed class BillingService : IBillingService
{
    private readonly BillingDbContext _db;
    private readonly IEventBus _eventBus;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        BillingDbContext db,
        IEventBus eventBus,
        TimeProvider timeProvider,
        ILogger<BillingService> logger)
    {
        _db = db;
        _eventBus = eventBus;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Invoice?> GenerateInvoiceForPeriodAsync(
        int periodYear,
        int periodMonth,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.Invoices
            .FirstOrDefaultAsync(i => i.PeriodYear == periodYear && i.PeriodMonth == periodMonth
                && i.Purpose == InvoicePurpose.Usage, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("[Billing] usage invoice already exists for period {Year}-{Month:00}, skipping",
                    periodYear, periodMonth);
            }
            return existing;
        }

        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.Status == SubscriptionStatus.Active, cancellationToken)
            .ConfigureAwait(false);
        if (subscription is null)
        {
            _logger.LogWarning("[Billing] no active subscription, skipping invoice");
            return null;
        }

        var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == subscription.PlanId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Plan {subscription.PlanId} not found.");


        var invoiceNumber = BuildUsageInvoiceNumber(periodYear, periodMonth);
        var invoice = Invoice.CreateDraft(invoiceNumber, periodYear, periodMonth, plan.Currency,
            InvoicePurpose.Usage, periodStartUtc: null, periodEndUtc: null);


        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("[Billing] generated draft invoice {InvoiceNumber} period {Year}-{Month:00} total={Total} {Currency}",
                invoice.InvoiceNumber, periodYear, periodMonth, invoice.SubtotalAmount.Amount, invoice.Currency);
        }
        return invoice;
    }

    public async Task<int> GenerateInvoicesAsync(
        int periodYear,
        int periodMonth,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.Invoices
            .AnyAsync(i => i.PeriodYear == periodYear && i.PeriodMonth == periodMonth
                && i.Purpose == InvoicePurpose.Usage, cancellationToken)
            .ConfigureAwait(false);
        if (existing) return 0;

        var inv = await GenerateInvoiceForPeriodAsync(periodYear, periodMonth, cancellationToken).ConfigureAwait(false);
        return inv is not null ? 1 : 0;
    }

    public async Task IssueInvoiceAsync(Guid invoiceId, DateTime? dueAtUtc, CancellationToken cancellationToken = default)
    {
        var invoice = await LoadInvoiceAsync(invoiceId, cancellationToken).ConfigureAwait(false);
        invoice.Issue(dueAtUtc);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Wallet> GetOrCreateWalletAsync(string currency, CancellationToken cancellationToken = default)
    {
        var wallet = await _db.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (wallet is null)
        {
            wallet = Wallet.Create(currency);
            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        return wallet;
    }

    public async Task<Invoice> CreateTopupInvoiceAsync(Guid topupRequestId, CancellationToken cancellationToken = default)
    {
        var request = await _db.TopupRequests
            .FirstOrDefaultAsync(r => r.Id == topupRequestId && r.Status == TopupRequestStatus.Pending, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Top-up request {topupRequestId} not found or not pending.");

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var invoiceNumber = BuildTopupInvoiceNumber(now, topupRequestId);

        var invoice = Invoice.CreateTopupDraft(
            invoiceNumber,
            now.Year,
            now.Month,
            request.Amount.Currency,
            request.Amount.Amount,
            $"WhatsApp wallet top-up ({request.Amount.Amount:0.##} {request.Amount.Currency})");

        invoice.Issue();
        _db.Invoices.Add(invoice);
        request.MarkInvoiced(invoice.Id, request.Note);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("[Billing] issued top-up invoice {InvoiceNumber} amount={Amount} {Currency}",
                invoice.InvoiceNumber, invoice.SubtotalAmount.Amount, invoice.Currency);
        }

        await _eventBus.PublishAsync(new InvoiceIssuedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredAt: now,
            CorrelationId: Guid.NewGuid().ToString(),
            Source: "Billing",
            InvoiceId: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            Amount: invoice.SubtotalAmount.Amount,
            Currency: invoice.Currency,
            DueAtUtc: invoice.DueAtUtc,
            PeriodYear: invoice.PeriodYear,
            PeriodMonth: invoice.PeriodMonth), cancellationToken).ConfigureAwait(false);

        return invoice;
    }

    public async Task MarkInvoicePaidAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await LoadInvoiceAsync(invoiceId, cancellationToken).ConfigureAwait(false);
        invoice.MarkPaid();

        if (invoice.Purpose == InvoicePurpose.Topup)
        {
            var topupRequest = await _db.TopupRequests
                .FirstOrDefaultAsync(r => r.InvoiceId == invoice.Id, cancellationToken)
                .ConfigureAwait(false);

            if (topupRequest is { Status: TopupRequestStatus.Invoiced })
            {
                var wallet = await _db.Wallets
                    .FirstOrDefaultAsync(w => w.Id == topupRequest.InvoiceId, cancellationToken)
                    .ConfigureAwait(false);

                if (wallet is null)
                {
                    wallet = Wallet.Create(invoice.Currency);
                    _db.Wallets.Add(wallet);
                }

                wallet.Credit(invoice.SubtotalAmount.Amount, WalletTransactionKind.Topup, "WhatsApp wallet top-up", topupRequest.Id.ToString());
                topupRequest.MarkCompleted();
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task VoidInvoiceAsync(Guid invoiceId, string? reason, CancellationToken cancellationToken = default)
    {
        var invoice = await LoadInvoiceAsync(invoiceId, cancellationToken).ConfigureAwait(false);
        invoice.Void(reason);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<Invoice> LoadInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        return await _db.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {invoiceId} not found.");
    }

    public async Task<Invoice?> CreateSubscriptionInvoiceAsync(
        Guid planId,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default)
    {
        var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == planId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Plan {planId} not found.");

        var termPrice = plan.TermPrice;
        if (termPrice.Amount <= 0m)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("[Billing] plan {PlanKey} term price is zero, no subscription invoice", plan.Key);
            }
            return null;
        }

        var periodStart = DateTime.SpecifyKind(periodStartUtc, DateTimeKind.Utc);
        var periodEnd = DateTime.SpecifyKind(periodEndUtc, DateTimeKind.Utc);
        var invoiceNumber = BuildSubscriptionInvoiceNumber(periodStart);

        var existing = await _db.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null) return existing;

        var invoice = Invoice.CreateDraft(invoiceNumber, periodStart.Year, periodStart.Month,
            plan.Currency, InvoicePurpose.Subscription, periodStart, periodEnd);
        invoice.AddLineItem(
            InvoiceLineItemKind.BaseFee,
            $"{plan.Name} — {plan.Interval} subscription ({periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd})",
            1m,
            termPrice.Amount);
        invoice.Issue();

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("[Billing] issued subscription invoice {InvoiceNumber} total={Total} {Currency}",
                invoice.InvoiceNumber, invoice.SubtotalAmount.Amount, invoice.Currency);
        }

        await _eventBus.PublishAsync(new InvoiceIssuedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredAt: _timeProvider.GetUtcNow().UtcDateTime,
            CorrelationId: Guid.NewGuid().ToString(),
            Source: "Billing",
            InvoiceId: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            Amount: invoice.SubtotalAmount.Amount,
            Currency: invoice.Currency,
            DueAtUtc: invoice.DueAtUtc,
            PeriodYear: invoice.PeriodYear,
            PeriodMonth: invoice.PeriodMonth), cancellationToken).ConfigureAwait(false);

        return invoice;
    }

    private static string BuildUsageInvoiceNumber(int periodYear, int periodMonth) =>
        $"USG-{periodYear}{periodMonth:00}-GLBL";

    private static string BuildSubscriptionInvoiceNumber(DateTime periodStartUtc) =>
        $"SUB-{periodStartUtc:yyyyMM}-GLBL";

    private static string BuildTopupInvoiceNumber(DateTime now, Guid topupRequestId)
    {
        var suffix = Convert.ToHexString(topupRequestId.ToByteArray(), 12, 4);
        return $"TOP-{now:yyyyMM}-GLBL-{suffix}";
    }
}