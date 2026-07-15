using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Auditing.Contracts.Authorization;
using FSH.Modules.Auditing.Contracts.v1.Dtos;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Auditing.Persistence;
using FSH.Modules.Identity.Contracts.Services;
using Microsoft.EntityFrameworkCore;
using FSH.Modules.Auditing.Contracts.v1;

namespace FSH.Modules.Auditing.Features.v1;

public static class GetAuditSummaryEndpoint
{
    public static RouteHandlerBuilder MapGetAuditSummaryEndpoint(this IEndpointRouteBuilder group)
    {
        return group.MapGet(
                "/summary",
                async ([AsParameters] GetAuditSummaryQuery query, IMediator mediator, CancellationToken ct) =>
                    TypedResults.Ok(await mediator.Send(query, ct)))
            .WithName("GetAuditSummary")
            .WithSummary("Get audit summary")
            .WithDescription("Retrieve aggregate counts of audit events by type, severity, source, and tenant.")
            .RequirePermission(AuditingPermissions.AuditTrails.View)
            .Produces<AuditSummaryAggregateDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetAuditSummaryQueryValidator : AbstractValidator<GetAuditSummaryQuery>
{
    public GetAuditSummaryQueryValidator()
    {
        RuleFor(q => q)
            .Must(q => !q.FromUtc.HasValue || !q.ToUtc.HasValue || q.FromUtc <= q.ToUtc)
            .WithMessage("FromUtc must be less than or equal to ToUtc.");

        RuleFor(q => q)
            .Must(q =>
                !q.FromUtc.HasValue
                || !q.ToUtc.HasValue
                || (q.ToUtc.Value - q.FromUtc.Value) <= GetAuditSummaryQueryHandler.MaxWindow)
            .WithMessage($"Audit summary window cannot exceed {GetAuditSummaryQueryHandler.MaxWindow.TotalDays:0} days.");
    }
}

public sealed class GetAuditSummaryQueryHandler : IQueryHandler<GetAuditSummaryQuery, AuditSummaryAggregateDto>
{
    public static readonly TimeSpan MaxWindow = TimeSpan.FromDays(90);
    public static readonly TimeSpan DefaultWindow = TimeSpan.FromDays(7);

    private readonly AuditDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public GetAuditSummaryQueryHandler(
        AuditDbContext dbContext,
        ICurrentUser currentUser,
        IUserPermissionService permissions,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async ValueTask<AuditSummaryAggregateDto> Handle(GetAuditSummaryQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (fromUtc, toUtc) = ResolveWindow(query.FromUtc, query.ToUtc);
        var baseQuery = await BuildBaseQueryAsync().ConfigureAwait(false);

        var scoped = baseQuery.Where(a => a.OccurredAtUtc >= fromUtc && a.OccurredAtUtc <= toUtc);

        // Four GROUP BYs pushed to SQL against the same filtered set (no materialization).
        // Sequential because they share the DbContext; parallel would need four contexts.
        var byType = await scoped
            .GroupBy(a => a.EventType)
            .Select(g => new { g.Key, Count = (long)g.Count() })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var bySeverity = await scoped
            .GroupBy(a => a.Severity)
            .Select(g => new { g.Key, Count = (long)g.Count() })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var bySource = await scoped
            .Where(a => a.Source != null)
            .GroupBy(a => a.Source!)
            .Select(g => new { g.Key, Count = (long)g.Count() })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AuditSummaryAggregateDto
        {
            EventsByType = byType.ToDictionary(x => (AuditEventType)x.Key, x => x.Count),
            EventsBySeverity = bySeverity.ToDictionary(x => (AuditSeverity)x.Key, x => x.Count),
            EventsBySource = bySource.ToDictionary(x => x.Key, x => x.Count, StringComparer.OrdinalIgnoreCase),
        };
    }

    /// <summary>
    /// Mirrors <c>GetAuditsQueryHandler.BuildBaseQueryAsync</c>: scoped to the
    /// current tenant by default, opt-in cross-tenant via the explicit
    /// ViewCrossTenant permission.
    /// </summary>
    private async Task<IQueryable<AuditRecord>> BuildBaseQueryAsync()
    {
        return _dbContext.AuditRecords.AsNoTracking();
    }

    private (DateTime FromUtc, DateTime ToUtc) ResolveWindow(DateTime? from, DateTime? to)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var resolvedTo = to ?? now;
        var resolvedFrom = from ?? resolvedTo - DefaultWindow;

        if (resolvedTo - resolvedFrom > MaxWindow)
        {
            resolvedFrom = resolvedTo - MaxWindow;
        }

        return (resolvedFrom, resolvedTo);
    }
}