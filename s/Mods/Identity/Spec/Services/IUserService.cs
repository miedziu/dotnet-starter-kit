using FSH.Framework.Shared.Storage;
using FSH.Mods.Identity.Spec.v1;
using System.Security.Claims;

namespace FSH.Mods.Identity.Spec.Services;

public interface IUserService
{
    Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null, CancellationToken ct = default);
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null, CancellationToken ct = default);
    Task<List<UserDto>> GetListAsync(CancellationToken ct);
    Task<int> GetCountAsync(CancellationToken ct);
    Task<UserDto> GetAsync(string userId, CancellationToken ct);
    Task ToggleStatusAsync(bool activateUser, string userId, CancellationToken ct);
    Task<string> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal, CancellationToken ct = default);
    Task<string> RegisterAsync(string firstName, string lastName, string email, string userName, string password, string confirmPassword, string phoneNumber, string origin, string[]? referralUsernames = null, CancellationToken ct = default);
    Task UpdateAsync(string userId, string firstName, string lastName, string phoneNumber, FileUploadRequest image, bool deleteCurrentImage, CancellationToken ct = default);
    Task DeleteAsync(string userId, CancellationToken ct = default);
    Task<string> ConfirmEmailAsync(string userId, string code, CancellationToken ct);
    Task AdminConfirmEmailAsync(string userId, CancellationToken ct = default);
    Task ResendConfirmationEmailAsync(string userId, string origin, CancellationToken ct = default);
    Task<string> ConfirmPhoneNumberAsync(string userId, string code, CancellationToken ct = default);

    // permisions
    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken ct = default);

    // passwords
    Task ForgotPasswordAsync(string email, string origin, CancellationToken ct);
    Task ResetPasswordAsync(string email, string password, string token, CancellationToken ct);
    Task<List<string>?> GetPermissionsAsync(string userId, CancellationToken ct);

    Task ChangePasswordAsync(string password, string newPassword, string confirmNewPassword, string userId, CancellationToken ct = default);
    Task<string> AssignRolesAsync(string userId, List<UserRoleDto> userRoles, CancellationToken ct);
    Task<List<UserRoleDto>> GetUserRolesAsync(string userId, CancellationToken ct);
}