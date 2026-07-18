using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Constants;
using FSH.Mod.Audit.Spec;
using FSH.Mod.Identity.Spec.Services;
using FSH.Mod.Identity.Spec.v1.Impersonation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace FSH.Mod.Identity.Features.v1.Impersonation;

public static class EndImpersonationEndpoint
{
    internal static RouteHandlerBuilder MapEndImpersonationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/impersonation/end",
            [Authorize] async Task<Results<Ok<TokenResponse>, ProblemHttpResult>>
            ([FromServices] IMediator mediator,
             CancellationToken ct) =>
            {
                var token = await mediator.Send(new EndImpersonationCommand(), ct);
                return TypedResults.Ok(token);
            })
            .WithName("EndImpersonation")
            .WithSummary("End user impersonation")
            .WithDescription("Returns a fresh access + refresh token for the original actor based on the act_sub/act_tenant claims embedded in the impersonation token. Callable by any authenticated impersonation session.")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class EndImpersonationCommandHandler
    : ICommandHandler<EndImpersonationCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly ISecurityAudit _securityAudit;
    private readonly ICurrentUser _currentUser;
    private readonly IRequestContext _requestContext;
    private readonly IImpersonationGrantService _grantService;
    private readonly ILogger<EndImpersonationCommandHandler> _logger;

    public EndImpersonationCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        ISecurityAudit securityAudit,
        ICurrentUser currentUser,
        IRequestContext requestContext,
        IImpersonationGrantService grantService,
        ILogger<EndImpersonationCommandHandler> logger)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _securityAudit = securityAudit;
        _currentUser = currentUser;
        _requestContext = requestContext;
        _grantService = grantService;
        _logger = logger;
    }

    public async ValueTask<TokenResponse> Handle(
        EndImpersonationCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUser.IsAuthenticated())
        {
            throw new UnauthorizedException();
        }

        var claims = _currentUser.GetUserClaims()?.ToList()
            ?? throw new UnauthorizedException();

        var actorUserId = claims.FirstOrDefault(c => c.Type == ClaimConstants.ActorSubject)?.Value;
        var jti = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            // Signed in but no act_sub claim (End called on a non-impersonation token): client error,
            // must be 4xx not CustomException's default 500.
            throw new CustomException(
                "current session is not an impersonation session",
                errors: null,
                System.Net.HttpStatusCode.BadRequest);
        }

        var impersonatedUserId = _currentUser.GetUserId().ToString();

        // Mark grant ended BEFORE issuing actor tokens so a racing JWT-hook request sees "ended" (safer than the reverse).
        // If MarkEnded fails we proceed anyway: the grant expires naturally and the hook treats Unknown states as revoked.
        if (!string.IsNullOrWhiteSpace(jti))
        {
            try
            {
                await _grantService.MarkEndedByJtiAsync(jti, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to mark impersonation grant ended for jti={Jti}. Actor swap will still proceed.",
                    jti);
            }
        }

        var actorClaimsResult = await _identityService
            .BuildClaimsForUserAsync(actorUserId, cancellationToken);

        if (actorClaimsResult is null)
        {
            throw new NotFoundException("original actor not found");
        }

        var (subject, actorClaims) = actorClaimsResult.Value;

        var token = await _tokenService.IssueAsync(subject, actorClaims, cancellationToken);
        await _identityService.StoreRefreshTokenAsync(subject, token.RefreshToken, token.RefreshTokenExpiresAt, cancellationToken);

        await _securityAudit.ImpersonationEndedAsync(
            actorUserId: actorUserId,
            targetUserId: impersonatedUserId,
            clientId: _requestContext.ClientId ?? "unknown",
            ct: cancellationToken);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Impersonation ended: actor {ActorUserId} returned from {TargetUserId} jti={Jti}",
                actorUserId, impersonatedUserId, jti ?? "<missing>");
        }

        return token;
    }
}