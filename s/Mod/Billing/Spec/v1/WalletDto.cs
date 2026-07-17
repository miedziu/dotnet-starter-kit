namespace FSH.Modules.Billing.Contracts.v1.Dtos;

public sealed record WalletDto(
    Guid Id,
    string Currency,
    decimal Balance,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<WalletTransactionDto> RecentTransactions);