using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Identity.Spec;
using FSH.Mods.Identity.Spec.Services;
using FSH.Mods.Identity.Spec.v1;
using FSH.Mods.Identity.Spec.v1.Impersonation;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mods.Identity.Features.v1.Impersonation;

public static class GetImpersonationGrantsEndpoint
{
    internal static RouteHandlerBuilder MapGetImpersonationGrantsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/impersonation/grants",
            async ([AsParameters] GetImpersonationGrantsQuery query,
                   IMediator mediator,
                   CancellationToken ct) =>
                TypedResults.Ok(await mediator.Send(query, ct)))
            .WithName("GetImpersonationGrants")
            .WithSummary("List impersonation grants")
            .WithDescription("Lists impersonation sessions scoped to what the caller can see. Tenant admins are limited to grants targeting their own tenant; root operators can filter by any tenant.")
            .RequirePermission(IdentityPermissions.Impersonation.View)
            .Produces<IReadOnlyList<ImpersonationGrantDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class GetImpersonationGrantsQueryValidator : AbstractValidator<GetImpersonationGrantsQuery>
{
    public const int MaxTake = 500;

    public GetImpersonationGrantsQueryValidator()
    {
        RuleFor(q => q.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxTake)
            .WithMessage($"Take must be between 1 and {MaxTake}.");
    }
}

public sealed class GetImpersonationGrantsQueryHandler(
    IImpersonationGrantService grantService)
    : IQueryHandler<GetImpersonationGrantsQuery, IReadOnlyList<ImpersonationGrantDto>>
{
    public async ValueTask<IReadOnlyList<ImpersonationGrantDto>> Handle(
        GetImpersonationGrantsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await grantService.ListAsync(
            status: request.Status,
            actorUserId: request.ActorUserId,
            take: request.Take,
            ct: cancellationToken).ConfigureAwait(false);
    }
}