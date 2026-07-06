using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Billing.Contracts.v1.Wallets;
using FSH.Modules.Billing.Data;
using FSH.Modules.Billing.Domain;
using Mediator;

namespace FSH.Modules.Billing.Features.v1.Wallets.CreateTopupRequest;

public sealed class CreateTopupRequestCommandHandler(
    BillingDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<CreateTopupRequestCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTopupRequestCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var requestedBy = currentUser.IsAuthenticated() ? currentUser.GetUserId().ToString() : null;
        var request = TopupRequest.Create(command.Amount, "USD", command.Note, requestedBy);
        db.TopupRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return request.Id;
    }
}