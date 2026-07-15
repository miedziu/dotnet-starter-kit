using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Auditing.Contracts.Authorization;
using FSH.Modules.Auditing.Contracts.v1;
using FSH.Modules.Auditing.Contracts.v1.Dtos;
using FSH.Modules.Auditing.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Auditing.Features.v1;

public static class GetAuditsByCorrelationEndpoint
{
    public static RouteHandlerBuilder MapGetAuditsByCorrelationEndpoint(this IEndpointRouteBuilder group)
    {
        return group.MapGet(
                "/by-correlation/{correlationId}",
                async (string correlationId, DateTime? fromUtc, DateTime? toUtc, IMediator mediator, CancellationToken ct) =>
                    TypedResults.Ok(await mediator.Send(new GetAuditsByCorrelationQuery
                    {
                        CorrelationId = correlationId,
                        FromUtc = fromUtc,
                        ToUtc = toUtc
                    }, ct)))
            .WithName("GetAuditsByCorrelation")
            .WithSummary("Get audit events by correlation id")
            .WithDescription("Retrieve audit events associated with a given correlation id.")
            .RequirePermission(AuditingPermissions.AuditTrails.View)
            .Produces<IEnumerable<AuditSummaryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetAuditsByCorrelationQueryHandler : IQueryHandler<GetAuditsByCorrelationQuery, IReadOnlyList<AuditSummaryDto>>
{
    private readonly AuditDbContext _dbContext;

    public GetAuditsByCorrelationQueryHandler(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<IReadOnlyList<AuditSummaryDto>> Handle(GetAuditsByCorrelationQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<AuditRecord> audits = _dbContext.AuditRecords
            .AsNoTracking()
            .Where(a => a.CorrelationId == query.CorrelationId);

        if (query.FromUtc.HasValue)
        {
            audits = audits.Where(a => a.OccurredAtUtc >= query.FromUtc.Value);
        }

        if (query.ToUtc.HasValue)
        {
            audits = audits.Where(a => a.OccurredAtUtc <= query.ToUtc.Value);
        }

        var list = await audits
            .OrderBy(a => a.OccurredAtUtc)
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