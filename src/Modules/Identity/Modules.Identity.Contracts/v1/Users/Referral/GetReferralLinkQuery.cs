using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users.Referral;

public record GetReferralLinkQuery(string UserId) : IQuery<ReferralLinkResponse>;

public record ReferralLinkResponse(string ReferralCode, string ReferralLink);
