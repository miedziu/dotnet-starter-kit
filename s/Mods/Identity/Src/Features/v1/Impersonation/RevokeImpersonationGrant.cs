using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Audit.Spec;
using FSH.Mods.Identity.Spec;
using FSH.Mods.Identity.Spec.Services;
using FSH.Mods.Identity.Spec.v1;
using FSH.Mods.Identity.Spec.v1.Impersonation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace FSH.Mods.Identity.Features.v1.Impersonation;

public static class RevokeImpersonationGrantEndpoint
{
    public sealed record Body(string? Reason);

    internal static RouteHandlerBuilder MapRevokeImpersonationGrantEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/impersonation/grants/{id:guid}/revoke",
            async (Guid id,
                   [FromBody] Body? body,
                   IMediator mediator,
                   CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(
                    new RevokeImpersonationGrantCommand(id, body?.Reason), ct)))
            .WithName("RevokeImpersonationGrant")
            .WithSummary("Revoke an impersonation grant")
            .WithDescription("Marks the grant as revoked. Subsequent requests carrying the impersonation token are rejected by the JWT validation hook within ~1 second (cache TTL).")
            .RequirePermission(IdentityPermissions.Impersonation.Revoke)
            .Produces<ImpersonationGrantDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed class RevokeImpersonationGrantCommandValidator : AbstractValidator<RevokeImpersonationGrantCommand>
{
    public RevokeImpersonationGrantCommandValidator()
    {
        RuleFor(p => p.GrantId)
            .NotEmpty();

        RuleFor(p => p.Reason)
            .MaximumLength(512)
            .When(p => !string.IsNullOrEmpty(p.Reason));
    }
}

public sealed class RevokeImpersonationGrantCommandHandler(
    IImpersonationGrantService grantService,
    ICurrentUser currentUser,
    ISecurityAudit securityAudit,
    IRequestContext requestContext,
    ILogger<RevokeImpersonationGrantCommandHandler> logger)
    : ICommandHandler<RevokeImpersonationGrantCommand, ImpersonationGrantDto>
{
    public async ValueTask<ImpersonationGrantDto> Handle(
        RevokeImpersonationGrantCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!currentUser.IsAuthenticated())
        {
            throw new UnauthorizedException();
        }

        var callerUserId = currentUser.GetUserId().ToString();

        // Enforce visibility before revoking: tenant admins may only revoke grants in their own
        // tenant. Cross-tenant grants return 404 (not 403) so existence isn't confirmed out of scope.
        var grant = await grantService.GetByIdAsync(request.GrantId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException("impersonation grant not found");

        var updated = await grantService.RevokeAsync(
            id: request.GrantId,
            revokedByUserId: callerUserId,
            revokedByUserName: currentUser.Name,
            reason: request.Reason,
            ct: cancellationToken).ConfigureAwait(false);

        // Surface revoke as a first-class security event, queryable alongside Start/End entries.
        // The audit Reason is the revocation reason, not the original impersonation reason.
        await securityAudit.ImpersonationEndedAsync(
            actorUserId: grant.ActorUserId,
            targetUserId: grant.ImpersonatedUserId,
            clientId: requestContext.ClientId ?? "unknown",
            ct: cancellationToken).ConfigureAwait(false);

        logger.LogWarning(
            "Impersonation grant revoked: grantId={GrantId} jti={Jti} revokedBy={RevokedBy} reason={Reason}",
            updated.Id, updated.Jti, callerUserId, request.Reason ?? "<none>");

        return updated;
    }
}