using FSH.Modules.Billing.Contracts.v1.Dtos;
using FSH.Modules.Billing.Contracts.v1.Wallets;
using FSH.Modules.Billing.Mappings;
using FSH.Modules.Billing.Services;
using Mediator;

namespace FSH.Modules.Billing.Features.v1.Wallets.GetMyWallet;

public sealed class GetMyWalletQueryHandler(
    IBillingService billingService)
    : IQueryHandler<GetMyWalletQuery, WalletDto>
{
    public async ValueTask<WalletDto> Handle(GetMyWalletQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var wallet = await billingService.GetOrCreateWalletAsync("USD", cancellationToken).ConfigureAwait(false);
        return wallet.ToDto();
    }
}