using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Audit.Spec;
using FSH.Mod.Audit.Spec.v1;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Audit.Features.v1;

public static class GetSecurityAuditsEndpoint
{
    public static RouteHandlerBuilder MapGetSecurityAuditsEndpoint(this IEndpointRouteBuilder group)
    {
        return group.MapGet(
                "/security",
                async (DateTime? fromUtc, DateTime? toUtc, IMediator mediator, CancellationToken ct) =>
                    TypedResults.Ok(await mediator.Send(new GetSecurityAuditsQuery
                    {
                        FromUtc = fromUtc,
                        ToUtc = toUtc
                    }, ct)))
            .WithName("GetSecurityAudits")
            .WithSummary("Get security audit events")
            .WithDescription("Retrieve audit events for security-relevant actions.")
            .RequirePermission(AuditingPermissions.AuditTrails.View)
            .Produces<IEnumerable<AuditSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetSecurityAuditsQueryHandler : IQueryHandler<GetSecurityAuditsQuery, IReadOnlyList<AuditSummaryDto>>
{
    private readonly AuditDbContext _dbContext;

    public GetSecurityAuditsQueryHandler(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IReadOnlyList<AuditSummaryDto>> Handle(GetSecurityAuditsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<AuditRecord> audits = _dbContext.AuditRecords
            .AsNoTracking()
            .Where(a => a.EventType == (int)AuditEventType.Security);

        if (query.FromUtc.HasValue)
        {
            audits = audits.Where(a => a.OccurredAtUtc >= query.FromUtc.Value);
        }

        if (query.ToUtc.HasValue)
        {
            audits = audits.Where(a => a.OccurredAtUtc <= query.ToUtc.Value);
        }

        var list = await audits
            .OrderByDescending(a => a.OccurredAtUtc)
            .Select(a => new AuditSummaryDto
            {
                Id = a.Id,
                OccurredAtUtc = a.OccurredAtUtc,
                EventType = (AuditEventType)a.EventType,
                Severity = (AuditSeverity)a.Severity,
                UserId = a.UserId,
                UserName = a.UserName,
                TraceId = a.TraceId,
                CorrelationId = a.CorrelationId,
                RequestId = a.RequestId,
                Source = a.Source,
                Tags = (AuditTag)a.Tags
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return list;
    }
}