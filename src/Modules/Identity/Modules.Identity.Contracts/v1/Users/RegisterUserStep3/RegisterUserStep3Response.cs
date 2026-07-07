namespace FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep3;

public record RegisterUserStep3Response(string UserId, string UserName, string Message = "Profile completed successfully.");