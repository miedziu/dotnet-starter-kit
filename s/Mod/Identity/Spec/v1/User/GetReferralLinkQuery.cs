using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users;

public record GetReferralLinkQuery(string UserId) : IQuery<ReferralLinkResponse>;

public record ReferralLinkResponse(string ReferralUsername, string ReferralLink);