using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Billing.Mappings;
using FSH.Mod.Billing.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Billing.Features.v1.Wallets;

public static class GetMyWalletEndpoint
{
    internal static RouteHandlerBuilder MapGetMyWalletEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/wallet/me",
                async (IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new GetMyWalletQuery(), ct)))
            .WithName("GetMyWallet")
            .WithSummary("Get the wallet for the current tenant")
            .RequirePermission(BillingPermissions.View);
    }
}

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