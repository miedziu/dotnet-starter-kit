using FSH.Framework.Core.Domain;
using FSH.Modules.Billing.Contracts;

namespace FSH.Modules.Billing.Domain;

/// <summary>
/// Priced side of a plan. The plan key matches the key used by quota configuration so a
/// plan named "pro" in QuotaOptions.Plans corresponds to the BillingPlan with Key "pro". Limits
/// come from QuotaOptions; prices and overage rates come from here.
/// </summary>
public sealed class BillingPlan : BaseEntity<Guid>
{
    public string Key { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public Money MonthlyBasePrice { get; private set; } = default!;
    public string Currency => MonthlyBasePrice.Currency;
    public PlanInterval Interval { get; private set; } = PlanInterval.Monthly;

    /// <summary>
    /// Flat price charged per yearly term. Only meaningful when <see cref="Interval"/> is
    /// <see cref="PlanInterval.Yearly"/>; <c>null</c> falls back to twelve times the monthly base
    /// price so a yearly plan can be configured without restating the discount.
    /// </summary>
    public Money? AnnualPrice { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private BillingPlan() { }

    public static BillingPlan Create(
        string key,
        string name,
        string currency,
        decimal monthlyBasePrice,
        PlanInterval interval = PlanInterval.Monthly,
        decimal? annualPrice = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        if (monthlyBasePrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthlyBasePrice), "Price cannot be negative.");
        }
        if (annualPrice is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(annualPrice), "Annual price cannot be negative.");
        }

        var plan = new BillingPlan
        {
            Id = Guid.CreateVersion7(),
#pragma warning disable CA1308 // Plan keys are canonical slugs stored lowercase (not security-sensitive)
            Key = key.ToLowerInvariant(),
#pragma warning restore CA1308
            Name = name,
            MonthlyBasePrice = new Money(monthlyBasePrice, currency),
            Interval = interval,
            AnnualPrice = annualPrice is { } a ? new Money(a, currency) : null,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return plan;
    }

    public void Update(
        string name,
        decimal monthlyBasePrice,
        PlanInterval interval = PlanInterval.Monthly,
        decimal? annualPrice = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (monthlyBasePrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monthlyBasePrice), "Price cannot be negative.");
        }
        if (annualPrice is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(annualPrice), "Annual price cannot be negative.");
        }

        Name = name;
        MonthlyBasePrice = new Money(monthlyBasePrice, Currency);
        Interval = interval;
        AnnualPrice = annualPrice is { } a ? new Money(a, Currency) : null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>Number of months the plan's billing interval covers (1 monthly, 12 yearly).</summary>
    public int TermMonths => Interval == PlanInterval.Yearly ? 12 : 1;

    /// <summary>
    /// Price charged for a single billing term: the monthly base price for monthly plans, or the
    /// annual price (falling back to twelve months) for yearly plans.
    /// </summary>
    public Money TermPrice =>
        Interval == PlanInterval.Yearly
            ? AnnualPrice ?? MonthlyBasePrice.Multiply(12m)
            : MonthlyBasePrice;
}