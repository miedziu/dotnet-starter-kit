using FSH.Modules.Billing.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Billing.Contracts.v1.Wallets;

public sealed record GetMyWalletQuery : IQuery<WalletDto>;