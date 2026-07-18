using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public record GetReferralLinkQuery(string UserId) : IQuery<ReferralLinkResponse>;

public record ReferralLinkResponse(string ReferralUsername, string ReferralLink);