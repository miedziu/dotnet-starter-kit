using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users.Referral;
using Mediator;

namespace FSH.Modules.Identity.Features.v1.Users.Referral;

public sealed class RecordReferralVisitCommandHandler(
    IReferralService referralService) : ICommandHandler<RecordReferralVisitCommand, RecordReferralVisitResponse>
{
    public async ValueTask<RecordReferralVisitResponse> Handle(RecordReferralVisitCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        var referrerName = await referralService.RecordReferralVisitAsync(command.ReferralCode, command.UserId, cancellationToken).ConfigureAwait(false);
        
        return new RecordReferralVisitResponse(referrerName != null, referrerName);
    }
}