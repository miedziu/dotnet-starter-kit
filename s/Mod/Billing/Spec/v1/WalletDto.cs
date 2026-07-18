namespace FSH.Mod.Billing.Spec.v1;

public sealed record WalletDto(
    Guid Id,
    string Currency,
    decimal Balance,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<WalletTransactionDto> RecentTransactions);