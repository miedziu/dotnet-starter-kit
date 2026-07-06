namespace FSH.Modules.Billing.Contracts.Dtos;

public sealed record WalletDto(
    Guid Id,
    string Currency,
    decimal Balance,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<WalletTransactionDto> RecentTransactions);