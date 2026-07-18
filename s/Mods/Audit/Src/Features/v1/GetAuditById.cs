using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Audit.Data;
using FSH.Mods.Audit.Spec;
using FSH.Mods.Audit.Spec.v1;
using FSH.Mods.Audit.Spec.v1.Audit;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FSH.Mods.Audit.Features.v1;

public static class GetAuditByIdEndpoint
{
    public static RouteHandlerBuilder MapGetAuditByIdEndpoint(this IEndpointRouteBuilder group)
    {
        return group.MapGet(
                "/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                    TypedResults.Ok(await mediator.Send(new GetAuditByIdQuery(id), ct)))
            .WithName("GetAuditById")
            .WithSummary("Get audit event by ID")
            .WithDescription("Retrieve full details for a single audit event.")
            .RequirePermission(AuditPermissions.AuditTrails.View)
            .Produces<AuditDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed class GetAuditByIdQueryHandler : IQueryHandler<GetAuditByIdQuery, AuditDetailDto>
{
    private readonly AuditDbContext _dbContext;

    public GetAuditByIdQueryHandler(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<AuditDetailDto> Handle(GetAuditByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var record = await _dbContext.AuditRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == query.Id, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            // KeyNotFoundException maps to 404 globally. Kept (not framework NotFoundException)
            // because audit exception-type fixtures and severity classification key off this type.
            throw new KeyNotFoundException($"Audit record {query.Id} not found.");
        }

        JsonElement payload;
        try
        {
            using var document = JsonDocument.Parse(record.PayloadJson);
            payload = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            payload = JsonDocument.Parse("{}").RootElement.Clone();
        }

        return new AuditDetailDto
        {
            Id = record.Id,
            OccurredAtUtc = record.OccurredAtUtc,
            ReceivedAtUtc = record.ReceivedAtUtc,
            EventType = (AuditEventType)record.EventType,
            Severity = (AuditSeverity)record.Severity,
            UserId = record.UserId,
            UserName = record.UserName,
            TraceId = record.TraceId,
            SpanId = record.SpanId,
            CorrelationId = record.CorrelationId,
            RequestId = record.RequestId,
            Source = record.Source,
            Tags = (AuditTag)record.Tags,
            Payload = payload
        };
    }
}