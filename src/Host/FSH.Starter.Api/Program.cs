using FSH.Framework.Web;
using FSH.Framework.Web.Modules;
using FSH.Framework.Web.Observability.Logging.Serilog;
using FSH.Modules.Auditing;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Auditing.Persistence;
using FSH.Modules.Billing;
using FSH.Modules.Billing.Contracts.v1.Invoices;
using FSH.Modules.Chat;
using FSH.Modules.Chat.Contracts.v1.Commands;
using FSH.Modules.Files;
using FSH.Modules.Files.Contracts.v1.Commands;
using FSH.Modules.Identity;
using FSH.Modules.Identity.Contracts.v1.Tokens;
using FSH.Modules.Identity.Features.v1.Tokens;
using FSH.Modules.Notifications;
using FSH.Modules.Notifications.Contracts.v1;
using FSH.Modules.Tickets;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using FSH.Modules.Webhooks;
using FSH.Modules.Webhooks.Contracts.v1.Subscription;
using FSH.Starter.Api;
using System.Text.Json.Serialization;

// Initialize static logger for early logging (before DI is built)
StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

// Serialize enums as string names (reads still accept names or integers). [Flags] enums (AuditTag, BodyCapture)
// opt back to numeric via their own NumericEnumConverter since comma-joined flag strings break bitwise consumers. Frontends mirror this as string unions.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

if (builder.Environment.IsProduction())
{
    static void Require(IConfiguration config, string key)
    {
        if (string.IsNullOrWhiteSpace(config[key]))
        {
            throw new InvalidOperationException($"Missing required configuration '{key}' in Production.");
        }
    }

    var config = builder.Configuration;
    Require(config, "DatabaseOptions:ConnectionString");
    Require(config, "CachingOptions:Redis");
    Require(config, "JwtOptions:SigningKey");
}

// Define all modules: (Name, ContractsType, RuntimeType)
var allModules = new[]
{
    ("Identity", typeof(GenerateTokenCommand), typeof(IdentityModule)),
    ("Auditing", typeof(AuditEnvelope), typeof(AuditingModule)),
    ("Billing", typeof(GenerateInvoicesCommand), typeof(BillingModule)),
    ("Chat", typeof(CreateChannelCommand), typeof(ChatModule)),
    ("Files", typeof(RequestUploadUrlCommand), typeof(FilesModule)),
    ("Notifications", typeof(MarkNotificationReadCommand), typeof(NotificationsModule)),
    ("Tickets", typeof(CreateTicketCommand), typeof(TicketsModule)),
    ("Webhooks", typeof(CreateWebhookSubscriptionCommand), typeof(WebhooksModule)),
};

// Register ALL module handlers with Mediator for source generator discovery
// (Mediator source generator needs inline types at compile time)
builder.Services.AddMediator(o =>
{
    o.ServiceLifetime = ServiceLifetime.Scoped;
    o.Assemblies = [
        typeof(GenerateTokenCommand),
        typeof(GenerateTokenCommandHandler),
        typeof(AuditEnvelope),
        typeof(AuditDbContext),
        typeof(GenerateInvoicesCommand),
        typeof(BillingModule),
        typeof(RequestUploadUrlCommand),
        typeof(FilesModule),
        typeof(CreateChannelCommand),
        typeof(ChatModule),
        typeof(MarkNotificationReadCommand),
        typeof(NotificationsModule),
        typeof(CreateTicketCommand),
        typeof(TicketsModule),
        typeof(CreateWebhookSubscriptionCommand),
        typeof(WebhooksModule),
    ];
});

// Get filtered module assemblies based on configuration (for AddModules only)
var moduleAssemblies = ModuleRegistrationHelper.GetFilteredModuleAssemblies(
    builder.Configuration, allModules);

builder.AddHeroPlatform(o =>
{
    o.EnableCaching = true;
    o.EnableMailing = true;
    o.EnableJobs = true;
    o.EnableSse = true;
    o.EnableRealtime = true;
});

builder.AddModules(moduleAssemblies);

// Self-heal deployments carrying retired per-module `{module}-outbox-dispatcher` Hangfire recurring jobs
// (the outbox is now dispatched by OutboxDispatcherHostedService). No-op once the storage is clean.
builder.Services.AddHostedService<OrphanedOutboxRecurringJobCleanupService>();

// Demo data is provisioned by the DbMigrator's `seed-demo` verb, not the API — the API never mutates data on startup.
// See src/Host/FSH.Starter.DbMigrator/README.md.

var app = builder.Build();

app.UseHeroPlatform(p =>
{
    p.MapModules = true;
    p.ServeStaticFiles = true;
    p.MapSseEndpoints = true;
    p.MapRealtime = true;
});

app.MapGet("/", () => Results.Ok(new { message = "hello world!" }))
   .WithTags("PlayGround")
   .AllowAnonymous();
await app.RunAsync();