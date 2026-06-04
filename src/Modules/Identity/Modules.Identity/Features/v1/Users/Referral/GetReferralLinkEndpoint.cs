using FSH.Modules.Identity.Contracts.v1.Users.Referral;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Users.Referral;

public static class GetReferralLinkEndpoint
{
    internal static RouteHandlerBuilder MapGetReferralLinkEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/referral-link", async (
            HttpContext context,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var userId = context.User?.Identity?.Name;
            var query = new GetReferralLinkQuery(userId ?? string.Empty);
            var result = await mediator.Send(query, cancellationToken);
            return TypedResults.Ok(result);
        })
        .WithName("GetReferralLink")
        .WithSummary("Get referral link")
        .Produces<ReferralLinkResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}