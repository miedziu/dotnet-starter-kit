namespace FSH.Modules.Identity.Contracts.v1.Users;

public record RegisterUserStep2Response(string UserId, bool RequiresProfileCompletion);