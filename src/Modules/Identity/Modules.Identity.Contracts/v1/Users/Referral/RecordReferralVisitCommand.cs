using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users.Referral;

public record RecordReferralVisitCommand(string UserId, string ReferralCode) : ICommand<RecordReferralVisitResponse>;

public record RecordReferralVisitResponse(bool Recorded, string? ReferrerName);
