using FSH.Modules.Billing.Contracts.v1.Wallets;
using FSH.Modules.Billing.Services;
using Mediator;

namespace FSH.Modules.Billing.Features.v1.Wallets.ApproveTopupRequest;

public sealed class ApproveTopupRequestCommandHandler(
    IBillingService billing)
    : ICommandHandler<ApproveTopupRequestCommand, Guid>
{
    public async ValueTask<Guid> Handle(ApproveTopupRequestCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // For root, operate on the request's own tenant; for non-root, callerTenantId equals request.TenantId.
        var invoice = await billing.CreateTopupInvoiceAsync(command.Id, cancellationToken)
            .ConfigureAwait(false);

        return invoice.Id;
    }
}