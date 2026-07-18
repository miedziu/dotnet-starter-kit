using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Audit.Data;
using FSH.Mods.Audit.Domain;
using FSH.Mods.Audit.Spec;
using FSH.Mods.Audit.Spec.v1;
using FSH.Mods.Audit.Spec.v1.Audit;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Audit.Features.v1;

public static class GetExceptionAuditsEndpoint
{
    public static RouteHandlerBuilder MapGetExceptionAuditsEndpoint(this IEndpointRouteBuilder group)
    {
        return group.MapGet(
                "/exceptions",
                async (DateTime? fromUtc, DateTime? toUtc, IMediator mediator, CancellationToken ct) =>
                    TypedResults.Ok(await mediator.Send(new GetExceptionAuditsQuery
                    {
                        FromUtc = fromUtc,
                        ToUtc = toUtc
                    }, ct)))
            .WithName("GetExceptionAudits")
            .WithSummary("Get exception audit events")
            .WithDescription("Retrieve audit events for unhandled exceptions.")
            .RequirePermission(AuditPermissions.AuditTrails.View)
            .Produces<IEnumerable<AuditSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetExceptionAuditsQueryHandler : IQueryHandler<GetExceptionAuditsQuery, IReadOnlyList<AuditSummaryDto>>
{
    private readonly AuditDbContext _dbContext;

    public GetExceptionAuditsQueryHandler(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IReadOnlyList<AuditSummaryDto>> Handle(GetExceptionAuditsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<AuditRecord> audits = _dbContext.AuditRecords
            .AsNoTracking()
            .Where(a => a.Severity == (byte)AuditSeverity.Error);

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