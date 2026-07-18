using Asp.Versioning;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Mod;
using FSH.Mod.Ticket.Data;
using FSH.Mod.Ticket.Features.v1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

[assembly: FshModule(typeof(FSH.Mod.Ticket.TicketModule), 700)]

namespace FSH.Mod.Ticket;

public sealed class TicketModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(TicketPermissions.All);

        builder.Services.AddHeroDbContext<TicketDbContext>();
        builder.Services.AddScoped<IDbInitializer, TicketDbInitializer>();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<TicketDbContext>(
                name: "db:tickets",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No custom middleware needed
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints
            .MapGroup("api/v{version:apiVersion}")
            .WithTags("Tickets")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        // Trash + comment routes register before the catch-all `{ticketId:guid}` GET so literal
        // segments win — minimal APIs match the first compatible pattern, so order matters.
        group.MapListTrashedTicketsEndpoint();
        group.MapAddTicketCommentEndpoint();
        group.MapListTicketCommentsEndpoint();

        group.MapRestoreTicketEndpoint();
        group.MapAssignTicketEndpoint();
        group.MapResolveTicketEndpoint();
        group.MapReopenTicketEndpoint();
        group.MapCloseTicketEndpoint();

        group.MapCreateTicketEndpoint();
        group.MapSearchTicketsEndpoint();
        group.MapUpdateTicketEndpoint();
        group.MapDeleteTicketEndpoint();
        group.MapGetTicketByIdEndpoint();
    }
}