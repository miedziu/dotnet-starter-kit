using Asp.Versioning;
using FluentValidation;
using FSH.Framework.Eventing;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Identity;
using FSH.Framework.Web.Modules;
using FSH.Mods.Notification.Data;
using FSH.Mods.Notification.Features.v1;
using FSH.Mods.Notification.Spec;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace FSH.Mods.Notification;

/// <summary>
/// Notification module: per-user inbox driven by integration events from other modules. Module
/// Order 750 places it BEFORE Chat (800) so its integration-event handlers are registered
/// before Chat starts publishing — handler registration is order-sensitive.
/// </summary>
public sealed class NotificationModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(NotificationPermissions.All);

        builder.Services.AddHeroDbContext<NotificationDbContext>();
        builder.Services.AddScoped<IDbInitializer, NotificationDbInitializer>();
        builder.Services.AddValidatorsFromAssembly(typeof(NotificationModule).Assembly);

        // Subscribe to cross-module integration events handled by this assembly.
        builder.Services.AddIntegrationEventHandlers(typeof(NotificationModule).Assembly);

        builder.Services.AddHealthChecks().AddDbContextCheck<NotificationDbContext>(
            name: "db:notifications",
            failureStatus: HealthStatus.Unhealthy);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/notifications")
            .WithTags("Notifications")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        // Literal routes first; /{id:guid}/read is the only param-route and lives last.
        group.MapListNotificationsEndpoint();              // GET /
        group.MapGetUnreadCountEndpoint();                 // GET /unread-count
        group.MapMarkAllNotificationsReadEndpoint();       // POST /read-all
        group.MapMarkNotificationReadEndpoint();           // POST /{id:guid}/read
    }
}