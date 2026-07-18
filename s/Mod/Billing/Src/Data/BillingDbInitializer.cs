using FSH.Framework.Persistence;
using FSH.Mod.Billing.Domain;
using FSH.Mod.Billing.Spec;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Mod.Billing.Data;

public sealed class BillingDbInitializer(
    BillingDbContext dbContext,
    ILogger<BillingDbInitializer> logger) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        if ((await dbContext.Database.GetPendingMigrationsAsync(ct).ConfigureAwait(false)).Any())
        {
            await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
            logger.LogInformation("[Billing] applied migrations");
        }
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        // Plans are a global catalogue; seed defaults once. "free" backs the trial
        // fallback; keys align with QuotaOptions plan keys so quota limits resolve.
        if (await dbContext.Plans.AnyAsync(ct).ConfigureAwait(false))
        {
            return;
        }

        dbContext.Plans.Add(BillingPlan.Create("free", "Free", "USD", 0m, interval: PlanInterval.Monthly));
        dbContext.Plans.Add(BillingPlan.Create("pro", "Pro", "USD", 29m, interval: PlanInterval.Monthly));
        dbContext.Plans.Add(BillingPlan.Create("pro-annual", "Pro (Annual)", "USD", 29m,
            interval: PlanInterval.Yearly, annualPrice: 290m));
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        logger.LogInformation("[Billing] seeded default plans (free, pro, pro-annual)");
    }
}