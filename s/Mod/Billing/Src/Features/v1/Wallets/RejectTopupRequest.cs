using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Billing.Contracts;
using FSH.Modules.Billing.Contracts.Authorization;
using FSH.Modules.Billing.Contracts.v1.Wallets;
using FSH.Modules.Billing.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FSH.Modules.Billing.Features.v1.Wallets;

public static class RejectTopupRequestEndpoint
{
    public sealed record RejectTopupRequestBody(string? Reason);

    internal static RouteHandlerBuilder MapRejectTopupRequestEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/wallet/topup-requests/{id:guid}/reject",
                async (Guid id, RejectTopupRequestBody? body, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new RejectTopupRequestCommand(id, body?.Reason), ct)))
            .WithName("RejectTopupRequest")
            .WithSummary("Reject a pending top-up request")
            .RequirePermission(BillingPermissions.Manage)
            .WithIdempotency();
    }
}

public sealed class RejectTopupRequestCommandValidator : AbstractValidator<RejectTopupRequestCommand>
{
    public RejectTopupRequestCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(512);
    }
}

public sealed class RejectTopupRequestCommandHandler(
    BillingDbContext db)
    : ICommandHandler<RejectTopupRequestCommand, Guid>
{
    public async ValueTask<Guid> Handle(RejectTopupRequestCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var request = await db.TopupRequests
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Top-up request {command.Id} not found.");

        if (request.Status != TopupRequestStatus.Pending)
        {
            throw new CustomException(
                $"Top-up request {command.Id} cannot be rejected because it is {request.Status} (only Pending requests can be rejected).",
                (IEnumerable<string>?)null,
                HttpStatusCode.Conflict);
        }

        request.Reject(command.Reason);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return request.Id;
    }
}