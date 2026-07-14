using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Constants;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FSH.Modules.Identity.Services;

internal sealed class UserStatusService(
    UserManager<FshUser> userManager,
    ICurrentUser currentUser,
    IAuditClient auditClient) : IUserStatusService
{
    // Soft-delete is functionally identical to deactivation — delegate so the same admin/self/last-admin
    // guards and audit pipeline apply uniformly to both DELETE /users/{id} and PATCH /users/{id}.
    public Task DeleteAsync(string userId, CancellationToken ct = default)
        => ToggleStatusAsync(activateUser: false, userId, ct);

    public async Task ToggleStatusAsync(bool activateUser, string userId, CancellationToken ct)
    {
        var context = await BuildToggleContextAsync(userId, activateUser, ct);

        await ValidateTogglePermissionsAsync(context, ct);

        ApplyStatusChange(context);

        await SaveAndAuditAsync(context, ct);
    }

    private async Task<ToggleStatusContext> BuildToggleContextAsync(
        string userId,
        bool activateUser,
        CancellationToken ct)
    {
        var actorId = currentUser.GetUserId();
        if (actorId == Guid.Empty)
        {
            throw new UnauthorizedException("authenticated user required to toggle status");
        }

        var actor = await userManager.FindByIdAsync(actorId.ToString())
            ?? throw new UnauthorizedException("current user not found");

        var targetUser = await userManager.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("User Not Found.");

        return new ToggleStatusContext(
            ActorId: actorId,
            Actor: actor,
            TargetUser: targetUser,
            ActivateUser: activateUser);
    }

    private async Task ValidateTogglePermissionsAsync(
        ToggleStatusContext context,
        CancellationToken ct)
    {
        if (!await userManager.IsInRoleAsync(context.Actor, RoleConstants.Admin))
        {
            await AuditPolicyFailureAsync(context, "ActorNotAdmin", ct);
            throw new ForbiddenException("Only administrators can change user status.");
        }

        if (!context.ActivateUser && context.ActorId.ToString() == context.TargetUser.Id)
        {
            await AuditPolicyFailureAsync(context, "SelfDeactivationBlocked", ct);
            throw new CustomException("Users cannot deactivate themselves.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        if (!context.ActivateUser && await userManager.IsInRoleAsync(context.TargetUser, RoleConstants.Admin))
        {
            await AuditPolicyFailureAsync(context, "AdminDeactivationBlocked", ct);
            throw new CustomException("Administrators cannot be deactivated.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        if (!context.ActivateUser)
        {
            await EnsureMinimumActiveAdminsAsync(context, ct);
        }
    }

    private async Task EnsureMinimumActiveAdminsAsync(
        ToggleStatusContext context,
        CancellationToken ct)
    {
        var activeAdmins = await userManager.GetUsersInRoleAsync(RoleConstants.Admin);
        if (!activeAdmins.Any(u => u.IsActive))
        {
            await AuditPolicyFailureAsync(context, "NoActiveAdmins", ct);
            throw new CustomException("Application must have at least one active administrator.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }
    }

    private static void ApplyStatusChange(ToggleStatusContext context)
    {
        if (context.ActivateUser)
        {
            context.TargetUser.Activate(context.ActorId.ToString());
        }
        else
        {
            context.TargetUser.Deactivate(context.ActorId.ToString(), "Status toggled by administrator");
        }
    }

    private async Task SaveAndAuditAsync(
        ToggleStatusContext context,
        CancellationToken ct)
    {
        var result = await userManager.UpdateAsync(context.TargetUser);
        if (!result.Succeeded)
        {
            throw new CustomException("Toggle status failed", result.Errors.Select(e => e.Description).ToList(), HttpStatusCode.BadRequest);
        }

        await auditClient.WriteActivityAsync(
            ActivityKind.Command,
            name: "ToggleUserStatus",
            statusCode: 204,
            durationMs: 0,
            captured: BodyCapture.None,
            requestSize: 0,
            responseSize: 0,
            requestPreview: new { actorId = context.ActorId.ToString(), targetUserId = context.TargetUser.Id, action = context.ActivateUser ? "activate" : "deactivate" },
            responsePreview: new { outcome = "success" },
            severity: AuditSeverity.Information,
            source: "Identity",
            ct: ct).ConfigureAwait(false);
    }

    private async Task AuditPolicyFailureAsync(
        ToggleStatusContext context,
        string reason,
        CancellationToken ct)
    {
        var claims = new Dictionary<string, object?>
        {
            ["actorId"] = context.ActorId.ToString(),
            ["targetUserId"] = context.TargetUser.Id,
            ["action"] = context.ActivateUser ? "activate" : "deactivate"
        };

        await auditClient.WriteSecurityAsync(
            SecurityAction.PolicyFailed,
            subjectId: context.ActorId.ToString(),
            reasonCode: reason,
            claims: claims,
            severity: AuditSeverity.Warning,
            source: "Identity",
            ct: ct).ConfigureAwait(false);
    }

    private sealed record ToggleStatusContext(
        Guid ActorId,
        FshUser Actor,
        FshUser TargetUser,
        bool ActivateUser);
}