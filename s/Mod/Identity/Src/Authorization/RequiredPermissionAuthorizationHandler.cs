using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Identity.Claims;
using FSH.Mod.Identity.Spec.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace FSH.Mod.Identity.Authorization;

public sealed class RequiredPermissionAuthorizationHandler(IUserService userService) : AuthorizationHandler<PermissionAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        var httpContext = context.Resource as HttpContext;
        var endpoint = context.Resource switch
        {
            HttpContext ctx => ctx.GetEndpoint(),
            Endpoint ep => ep,
            _ => null,
        };

        // IMPORTANT: resolve IRequiredPermissionMetadata from FSH.Framework.Shared.Identity.Authorization (the
        // interface the attribute implements) — a duplicate would silently fail-open every .RequirePermission().
        var requiredPermissions = endpoint?.Metadata.GetMetadata<IRequiredPermissionMetadata>()?.RequiredPermissions;
        if (requiredPermissions == null)
        {
            // there are no permission requirements set by the endpoint
            // hence, authorize requests
            context.Succeed(requirement);
            return;
        }

        var ct = httpContext?.RequestAborted ?? CancellationToken.None;
        if (context.User?.GetUserId() is { } userId && await userService.HasPermissionAsync(userId, requiredPermissions.First(), ct).ConfigureAwait(false))
        {
            context.Succeed(requirement);
        }
    }
}