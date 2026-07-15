using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Identity.Contracts.Authorization;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Identity.Contracts.v1.Impersonation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FSH.Modules.Identity.Features.v1.Impersonation;

public static class StartImpersonationEndpoint
{
    internal static RouteHandlerBuilder MapStartImpersonationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/impersonation/start",
            async Task<Results<Ok<ImpersonationResponse>, ProblemHttpResult>>
            ([FromBody] StartImpersonationCommand command,
             [FromServices] IMediator mediator,
             CancellationToken ct) =>
            {
                var response = await mediator.Send(command, ct);
                return TypedResults.Ok(response);
            })
            .WithName("StartImpersonation")
            .WithSummary("Start user impersonation")
            .WithDescription("Issues a short-lived access token representing the target user. The token carries actor claims (act_sub, act_tenant) identifying the original caller. Platform operators (root tenant) may impersonate any user; tenant admins can only impersonate users within their own tenant. No refresh token is issued.")
            .RequirePermission(IdentityPermissions.Users.Impersonate)
            .Produces<ImpersonationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed class StartImpersonationCommandValidator : AbstractValidator<StartImpersonationCommand>
{
    /// <summary>
    /// Upper bound on impersonation token lifetime — the server will silently
    /// cap to this even if the validator passes, but we reject obvious abuse
    /// (negative, zero, or absurd values) up front.
    /// </summary>
    public const int MaxImpersonationMinutes = 60;

    public StartImpersonationCommandValidator()
    {
        RuleFor(p => p.TargetUserId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty();

        RuleFor(p => p.DurationMinutes!.Value)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxImpersonationMinutes)
            .WithMessage($"Duration must be between 1 and {MaxImpersonationMinutes} minutes.")
            .When(p => p.DurationMinutes.HasValue);
    }
}

public sealed class StartImpersonationCommandHandler
    : ICommandHandler<StartImpersonationCommand, ImpersonationResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly ISecurityAudit _securityAudit;
    private readonly ICurrentUser _currentUser;
    private readonly IRequestContext _requestContext;
    private readonly IImpersonationGrantService _grantService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<StartImpersonationCommandHandler> _logger;

    public StartImpersonationCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        ISecurityAudit securityAudit,
        ICurrentUser currentUser,
        IRequestContext requestContext,
        IImpersonationGrantService grantService,
        TimeProvider timeProvider,
        ILogger<StartImpersonationCommandHandler> logger)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _securityAudit = securityAudit;
        _currentUser = currentUser;
        _requestContext = requestContext;
        _grantService = grantService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async ValueTask<ImpersonationResponse> Handle(
        StartImpersonationCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUser.IsAuthenticated())
        {
            throw new UnauthorizedException();
        }

        var actorUserId = _currentUser.GetUserId().ToString();
        var actorUserName = _currentUser.Name;

        // Prevent self-impersonation (pointless, confuses the audit trail). Caller error → explicit 4xx,
        // not the 500 CustomException defaults to.
        if (string.Equals(actorUserId, request.TargetUserId, StringComparison.Ordinal))
        {
            throw new CustomException("cannot impersonate yourself", errors: null, System.Net.HttpStatusCode.BadRequest);
        }

        // Prevent nesting: if the caller is already impersonating, require end-impersonation first.
        var callerClaims = _currentUser.GetUserClaims();
        if (callerClaims is not null
            && callerClaims.Any(c => c.Type == ClaimConstants.ActorSubject))
        {
            throw new CustomException(
                "end current impersonation before starting a new one",
                errors: null,
                System.Net.HttpStatusCode.BadRequest);
        }

        var targetClaimsResult = await _identityService
            .BuildClaimsForUserAsync(request.TargetUserId, cancellationToken);

        if (targetClaimsResult is null)
        {
            throw new NotFoundException("target user not found");
        }

        var (subject, claims) = targetClaimsResult.Value;
        var targetUserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            ?? claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;

        // Strip the auto-generated jti from BuildClaimsForUserAsync and inject our own, so the persisted
        // ImpersonationGrant row and the issued JWT share the same jti.
        var jti = Guid.NewGuid().ToString("N");
        var impersonationClaims = claims
            .Where(c => c.Type != JwtRegisteredClaimNames.Jti)
            .Concat(
            [
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                // RFC 8693 actor claims so the issued token carries who is acting.
                new Claim(ClaimConstants.ActorSubject, actorUserId)
            ])
            .ToList();

        // Cap the caller-supplied duration server-side (defense in depth: the validator already rejects
        // out-of-range, but a future caller bypassing it must not escape the cap).
        var lifetime = request.DurationMinutes is { } minutes
            ? TimeSpan.FromMinutes(Math.Clamp(minutes, 1, StartImpersonationCommandValidator.MaxImpersonationMinutes))
            : (TimeSpan?)null;

        var startedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var (accessToken, expiresAt) = await _tokenService.IssueAccessOnlyAsync(
            subject, impersonationClaims, lifetime, cancellationToken);

        // Persist the grant AFTER issuance so a failed issue leaves no orphan grant. CreateAsync primes the
        // cache so the JWT validation hook sees status=Active on the next request without a DB hit.
        await _grantService.CreateAsync(new CreateGrantInput(
            Jti: jti,
            ActorUserId: actorUserId,
            ActorUserName: actorUserName,
            ImpersonatedUserId: subject,
            ImpersonatedUserName: targetUserName,
            Reason: request.Reason ?? string.Empty,
            StartedAtUtc: startedAtUtc,
            ExpiresAtUtc: expiresAt,
            ClientId: _requestContext.ClientId,
            IpAddress: _requestContext.IpAddress,
            UserAgent: _requestContext.UserAgent), cancellationToken);

        await _securityAudit.ImpersonationStartedAsync(
            actorUserId: actorUserId,
            targetUserId: subject,
            clientId: _requestContext.ClientId ?? "unknown",
            ip: _requestContext.IpAddress ?? "unknown",
            userAgent: _requestContext.UserAgent ?? "unknown",
            reason: request.Reason ?? string.Empty,
            ct: cancellationToken);

        _logger.LogWarning(
            "Impersonation started: actor {ActorUserId} -> target {TargetUserId} jti={Jti}",
            actorUserId, subject, jti);

        return new ImpersonationResponse(
            AccessToken: accessToken,
            AccessTokenExpiresAt: expiresAt,
            ActorUserId: actorUserId,
            ImpersonatedUserId: subject);
    }
}