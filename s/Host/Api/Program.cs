using FSH.Framework.Web;
using FSH.Framework.Web.Modules;
using FSH.Framework.Web.Observability.Logging.Serilog;
using FSH.Mods.Audit;
using FSH.Mods.Audit.Data;
using FSH.Mods.Audit.Spec;
using FSH.Mods.Billing;
using FSH.Mods.Billing.Spec.v1.Invoice;
using FSH.Mods.Chat;
using FSH.Mods.Chat.Spec.v1.Channel;
using FSH.Mods.File;
using FSH.Mods.File.Spec.v1.File;
using FSH.Mods.Identity;
using FSH.Mods.Identity.Features.v1.Tokens;
using FSH.Mods.Identity.Spec.v1.Token;
using FSH.Mods.Notification;
using FSH.Mods.Notification.Spec.v1.Notifications;
using FSH.Mods.Ticket;
using FSH.Mods.Ticket.Spec.v1.Ticket;
using FSH.Mods.Webhook;
using FSH.Mods.Webhook.Spec.v1.Subscription;
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
    ("Audit", typeof(AuditEnvelope), typeof(AuditModule)),
    ("Billing", typeof(GenerateInvoicesCommand), typeof(BillingModule)),
    ("Chat", typeof(CreateChannelCommand), typeof(ChatModule)),
    ("File", typeof(RequestUploadUrlCommand), typeof(FileModule)),
    ("Notification", typeof(MarkNotificationReadCommand), typeof(NotificationModule)),
    ("Ticket", typeof(CreateTicketCommand), typeof(TicketModule)),
    ("Webhook", typeof(CreateWebhookSubscriptionCommand), typeof(WebhookModule)),
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
        typeof(FileModule),
        typeof(CreateChannelCommand),
        typeof(ChatModule),
        typeof(MarkNotificationReadCommand),
        typeof(NotificationModule),
        typeof(CreateTicketCommand),
        typeof(TicketModule),
        typeof(CreateWebhookSubscriptionCommand),
        typeof(WebhookModule),
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
// See s/Host/DbMigrator/README.md.

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