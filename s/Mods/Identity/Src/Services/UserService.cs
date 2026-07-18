using FSH.Framework.Shared.Storage;
using FSH.Mods.Identity.Spec.Services;
using FSH.Mods.Identity.Spec.v1;
using System.Security.Claims;

namespace FSH.Mods.Identity.Services;

/// <summary>
/// Facade service that delegates to focused single-responsibility services.
/// Maintained for backward compatibility with existing consumers.
/// </summary>
internal sealed class UserService(
    IUserRegistrationService registrationService,
    IUserProfileService profileService,
    IUserStatusService statusService,
    IUserRoleService roleService,
    IUserPasswordService passwordService,
    IUserPermissionService permissionService) : IUserService
{
    // Registration operations (delegated to IUserRegistrationService)
    public Task<string> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string userName,
        string password,
        string confirmPassword,
        string phoneNumber,
        string origin,
        string[]? referralUsernames = null,
        CancellationToken ct = default)
        => registrationService.RegisterAsync(firstName, lastName, email, userName, password, confirmPassword, phoneNumber, origin, referralUsernames, ct);

    public Task<string> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal, CancellationToken ct = default)
        => registrationService.GetOrCreateFromPrincipalAsync(principal, ct);

    public Task<string> ConfirmEmailAsync(string userId, string code, CancellationToken ct)
        => registrationService.ConfirmEmailAsync(userId, code, ct);

    public Task AdminConfirmEmailAsync(string userId, CancellationToken ct = default)
        => registrationService.AdminConfirmEmailAsync(userId, ct);

    public Task ResendConfirmationEmailAsync(string userId, string origin, CancellationToken ct = default)
        => registrationService.ResendConfirmationEmailAsync(userId, origin, ct);

    public Task<string> ConfirmPhoneNumberAsync(string userId, string code, CancellationToken ct = default)
        => registrationService.ConfirmPhoneNumberAsync(userId, code, ct);

    // Profile operations (delegated to IUserProfileService)
    public Task<UserDto> GetAsync(string userId, CancellationToken ct)
        => profileService.GetAsync(userId, ct);

    public Task<List<UserDto>> GetListAsync(CancellationToken ct)
        => profileService.GetListAsync(ct);

    public Task<int> GetCountAsync(CancellationToken ct)
        => profileService.GetCountAsync(ct);

    public Task UpdateAsync(string userId, string firstName, string lastName, string phoneNumber, FileUploadRequest image, bool deleteCurrentImage, CancellationToken ct = default)
        => profileService.UpdateAsync(userId, firstName, lastName, phoneNumber, image, deleteCurrentImage, ct);

    public Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null, CancellationToken ct = default)
        => profileService.ExistsWithEmailAsync(email, exceptId, ct);

    public Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default)
        => profileService.ExistsWithNameAsync(name, ct);

    public Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null, CancellationToken ct = default)
        => profileService.ExistsWithPhoneNumberAsync(phoneNumber, exceptId, ct);

    // Status operations (delegated to IUserStatusService)
    public Task ToggleStatusAsync(bool activateUser, string userId, CancellationToken ct)
        => statusService.ToggleStatusAsync(activateUser, userId, ct);

    public Task DeleteAsync(string userId, CancellationToken ct = default)
        => statusService.DeleteAsync(userId, ct);

    // Role operations (delegated to IUserRoleService)
    public Task<string> AssignRolesAsync(string userId, List<UserRoleDto> userRoles, CancellationToken ct)
        => roleService.AssignRolesAsync(userId, userRoles, ct);

    public Task<List<UserRoleDto>> GetUserRolesAsync(string userId, CancellationToken ct)
        => roleService.GetUserRolesAsync(userId, ct);

    // Password operations (delegated to IUserPasswordService)
    public Task ForgotPasswordAsync(string email, string origin, CancellationToken ct)
        => passwordService.ForgotPasswordAsync(email, origin, ct);

    public Task ResetPasswordAsync(string email, string password, string token, CancellationToken ct)
        => passwordService.ResetPasswordAsync(email, password, token, ct);

    public Task ChangePasswordAsync(string password, string newPassword, string confirmNewPassword, string userId, CancellationToken ct = default)
        => passwordService.ChangePasswordAsync(password, newPassword, confirmNewPassword, userId, ct);

    // Permission operations (delegated to IUserPermissionService)
    public Task<List<string>?> GetPermissionsAsync(string userId, CancellationToken ct)
        => permissionService.GetPermissionsAsync(userId, ct);

    public Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken ct = default)
        => permissionService.HasPermissionAsync(userId, permission, ct);
}