using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Billing.Contracts.Authorization;
using FSH.Modules.Billing.Contracts.v1.Wallets;
using FSH.Modules.Billing.Data;
using FSH.Modules.Billing.Domain;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Billing.Features.v1.Wallets;

public static class CreateTopupRequestEndpoint
{
    internal static RouteHandlerBuilder MapCreateTopupRequestEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/wallet/topup-requests",
                async (CreateTopupRequestCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateTopupRequest")
            .WithSummary("Submit a wallet top-up request for the current tenant")
            .RequirePermission(BillingPermissions.View)
            .WithIdempotency();
    }
}

public sealed class CreateTopupRequestCommandValidator : AbstractValidator<CreateTopupRequestCommand>
{
    public CreateTopupRequestCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0m).LessThanOrEqualTo(1_000_000m);
        RuleFor(x => x.Note).MaximumLength(512);
    }
}

public sealed class CreateTopupRequestCommandHandler(
    BillingDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<CreateTopupRequestCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTopupRequestCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var requestedBy = currentUser.IsAuthenticated() ? currentUser.GetUserId().ToString() : null;
        var request = TopupRequest.Create(command.Amount, "USD", command.Note, requestedBy);
        db.TopupRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return request.Id;
    }
}