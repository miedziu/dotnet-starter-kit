# Module: Billing

Plans, subscriptions, usage metering, monthly invoicing. **Manual payment marking — no payment provider.** Module `Order = 500`.

**Entities:** `BillingPlan`, `Subscription`, `Invoice` (+ `InvoiceLineItem`), `UsageSnapshot`. **`BillingDbContext : DbContext`** (NOT `BaseDbContext`) — billing lives in main DB, filtered in query services. Contracts = DTOs; `IBillingService`/`IUsageReporter` are internal.

**Areas:** Plans, Subscriptions, Invoices (generate/issue/mark-paid/void), Usage (capture/get). Monthly invoice job (`5 0 1 * *`).

## Gotchas

- **`BillingPlan`** — platform-wide catalogue rows. Plan's `Key` matches config key (e.g. `"pro"`): prices/overage from plan.
- **Invoice state machine** — `Draft → Issued → Paid | Void`. Line items only addable in Draft; Paid invoice can't be voided; totals recompute on add; Issue defaults due = +14 days.
- **Usage metering is idempotent** — `IUsageReporter.CaptureForPeriodAsync` persists one `UsageSnapshot` per period, so invoicing math is reproducible after mid-period plan change.