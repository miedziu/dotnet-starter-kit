using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Users.Referral;
using Mediator;

namespace FSH.Modules.Identity.Features.v1.Users.Referral;

public sealed class GetReferralLinkQueryHandler(
    IReferralService referralService) : IQueryHandler<GetReferralLinkQuery, ReferralLinkResponse>
{
    public async ValueTask<ReferralLinkResponse> Handle(GetReferralLinkQuery query, CancellationToken cancellationToken)
    {
        var userId = query.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("User ID is required");
        }
        
        var code = await referralService.GetOrCreateReferralCodeAsync(userId, cancellationToken).ConfigureAwait(false);
        
        // Build the referral link - using /ref?c=CODE format
        var link = $"/ref?c={code}";
        
        return new ReferralLinkResponse(code, link);
    }
}