using FSH.Framework.Persistence;
using FSH.Framework.Web;
using FSH.Framework.Web.Modules;
using FSH.Framework.Web.Observability.Logging.Serilog;
using FSH.Modules.Auditing;
using FSH.Modules.Auditing.Contracts;
using FSH.Modules.Auditing.Persistence;
using FSH.Modules.Billing;
using FSH.Modules.Billing.Contracts;
using FSH.Modules.Chat;
using FSH.Modules.Chat.Contracts.v1.Commands;
using FSH.Modules.Files;
using FSH.Modules.Files.Contracts.v1.Commands;
using FSH.Modules.Identity;
using FSH.Modules.Identity.Contracts.v1.Tokens;
using FSH.Modules.Identity.Data;
using FSH.Modules.Identity.Features.v1.Tokens;
using FSH.Modules.Notifications;
using FSH.Modules.Notifications.Contracts.v1;
using FSH.Modules.Tickets;
using FSH.Modules.Tickets.Contracts;
using FSH.Modules.Webhooks;
using FSH.Modules.Webhooks.Contracts.v1.Subscription;
using FSH.Starter.DbMigrator;
using FSH.Starter.DbMigrator.DemoSeed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

// FSH DbMigrator — one-shot console that migrates every DB to head, optionally seeds, then exits 0/1.
// Runs as a deployment step (not at API startup) so it can use an elevated-DDL connection string. Verbs: see MigratorCommand.HelpText.

// Initialize static logger for early logging (before DI is built)
StaticLogger.EnsureInitialized();

var cli = MigratorCommand.Parse(args);
if (cli.Help)
{
    await Console.Out.WriteLineAsync(MigratorCommand.HelpText).ConfigureAwait(false);
    return 0;
}
var builder = Host.CreateApplicationBuilder(args);

// Disable build-time DI validation: auto-on in Development, it walks ALL descriptors incl. handlers this
// reduced-graph process never invokes (Chat→IHubContext, Identity→IMailService) and throws — false positive.
builder.ConfigureContainer(new DefaultServiceProviderFactory(
    new ServiceProviderOptions { ValidateOnBuild = false, ValidateScopes = false }));

// Under dotnet run the cwd is the project folder but appsettings.json is copied to the output dir,
// so load it from AppContext.BaseDirectory for IdentityModule's JwtOptions to validate.
builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true);
builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, $"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true);

// Re-add environment variables and command line args so they maintain priority over the manually added JSON files.
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);

// IdentityModule's JwtOptions.ValidateOnStart() trips on the empty SigningKey in base appsettings, but
// the migrator never mints JWTs. Inject a labelled placeholder only when nothing real is configured.
if (string.IsNullOrWhiteSpace(builder.Configuration["JwtOptions:SigningKey"]))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["JwtOptions:SigningKey"] = "fsh-dbmigrator-placeholder-never-mints-tokens-32+",
        ["JwtOptions:Issuer"] = builder.Configuration["JwtOptions:Issuer"] ?? "fsh.local",
        ["JwtOptions:Audience"] = builder.Configuration["JwtOptions:Audience"] ?? "fsh.clients",
    });
}

// Register ModuleOptions for IOptions<ModuleOptions> injection (used by DemoSeeder)
builder.Services.Configure<ModuleOptions>(builder.Configuration.GetSection("ModuleOptions"));

// Fail-fast with one clear line if DatabaseOptions__ConnectionString is unset, rather than letting
// host-build-time option validation throw a stack trace.
if (string.IsNullOrWhiteSpace(builder.Configuration["DatabaseOptions:ConnectionString"]))
{
    await Console.Error.WriteLineAsync(
        "[migrator] FAILED: DatabaseOptions:ConnectionString is empty — refusing to run against an unconfigured target. "
        + "Set DatabaseOptions__ConnectionString to an elevated-DDL connection string before invoking the migrator.")
        .ConfigureAwait(false);
    return 1;
}

// Define all modules: (Name, ContractsType, RuntimeType)
var allModules = new[]
{
    ("Identity", typeof(GenerateTokenCommand), typeof(IdentityModule)),
    ("Auditing", typeof(AuditEnvelope), typeof(AuditingModule)),
    ("Files", typeof(RequestUploadUrlCommand), typeof(FilesModule)),
    ("Webhooks", typeof(CreateWebhookSubscriptionCommand), typeof(WebhooksModule)),
    ("Billing", typeof(BillingContractsMarker), typeof(BillingModule)),
    ("Tickets", typeof(TicketsContractsMarker), typeof(TicketsModule)),
    ("Chat", typeof(CreateChannelCommand), typeof(ChatModule)),
    ("Notifications", typeof(MarkNotificationReadCommand), typeof(NotificationsModule))
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
        typeof(CreateWebhookSubscriptionCommand),
        typeof(WebhooksModule),
        typeof(BillingContractsMarker),
        typeof(BillingModule),
        typeof(TicketsContractsMarker),
        typeof(TicketsModule),
        typeof(RequestUploadUrlCommand),
        typeof(FilesModule),
        typeof(CreateChannelCommand),
        typeof(ChatModule),
        typeof(MarkNotificationReadCommand),
        typeof(NotificationsModule),
    ];
});

// Get filtered module assemblies based on configuration (for AddModules only)
var moduleAssemblies = ModuleRegistrationHelper.GetFilteredModuleAssemblies(
    builder.Configuration, allModules);

// Disable runtime-only concerns; persistence stay on so DbInitializers resolve. Caching
// stays on because some modules' ctor wiring touches IDistributedCache (in-memory fallback if no Redis).
builder.AddHeroPlatform(o =>
{
    o.EnableOpenTelemetry = false;
    o.EnableCors = false;
    o.EnableOpenApi = false;
    o.EnableJobs = false;
    o.EnableMailing = false;
    o.EnableSse = false;
    o.EnableRealtime = false;
    o.EnableFeatureFlags = false;
    o.EnableIdempotency = false;
    o.EnableCaching = true;
});

builder.AddModules(moduleAssemblies);

// TenantProvisioningService needs IJobService, but Hangfire's is gated behind EnableJobs (off here).
// Provide a throwing no-op so the DI graph resolves; the migration code paths don't enqueue jobs.
builder.Services.AddSingleton<FSH.Framework.Jobs.Services.IJobService, NoOpJobService>();

// DemoSeeder is opt-in via the `seed-demo` verb. Register unconditionally so
// the DI graph is satisfied; the verb dispatch below decides whether to call it.
builder.Services.AddScoped<DemoSeeder>();

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<MigratorCommand>>();

// Start the host so logging providers / option validators initialise.
await host.StartAsync().ConfigureAwait(false);

try
{
    // ── Step 0 — wait for the database to come up ────────────────────────
    // Postgres may still be initialising on cold-start; exp. backoff (≤2 min), then TimeoutException + exit 1.
    var connectionString = host.Services.GetRequiredService<IConfiguration>()["DatabaseOptions:ConnectionString"]
        ?? throw new InvalidOperationException("DatabaseOptions:ConnectionString is not configured.");
    await Console.Out.WriteLineAsync("[migrator] waiting for postgres…").ConfigureAwait(false);
    await PostgresMigratorLock.WaitForDatabaseAsync(connectionString, logger, CancellationToken.None)
        .ConfigureAwait(false);
    await Console.Out.WriteLineAsync("[migrator] postgres ready").ConfigureAwait(false);

    // Log the connected role + database so a misconfigured low-priv connection string surfaces now,
    // not as "permission denied for schema public" during MigrateAsync.
    await LogConnectionIdentityAsync(connectionString).ConfigureAwait(false);

    // ── Step 0b — acquire the advisory lock ──────────────────────────────
    // Session-level lock: concurrent runs block here; auto-releases on connection close (no orphan on crash).
    await Console.Out.WriteLineAsync("[migrator] acquiring advisory lock…").ConfigureAwait(false);
    await using var migratorLock = await PostgresMigratorLock
        .AcquireAsync(connectionString, logger, CancellationToken.None)
        .ConfigureAwait(false);
    await Console.Out.WriteLineAsync("[migrator] advisory lock acquired").ConfigureAwait(false);

    // ── Step 1 — catalog ───────────────────────────────────────────
    // Always applied first: the migrator below reads every migration out of this database.
    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var pending = (await db.Database.GetPendingMigrationsAsync(CancellationToken.None)
            .ConfigureAwait(false)).ToList();

        if (cli.Command == "list-pending")
        {
            await Console.Out.WriteLineAsync(string.Create(
                CultureInfo.InvariantCulture,
                $"[catalog] {pending.Count} pending migration(s)"))
                .ConfigureAwait(false);
            foreach (var name in pending)
            {
                await Console.Out.WriteLineAsync($"  · {name}").ConfigureAwait(false);
            }
        }
        else if (pending.Count > 0)
        {
            await Console.Out.WriteLineAsync(string.Create(
                CultureInfo.InvariantCulture,
                $"[catalog] applying {pending.Count} migration(s)…"))
                .ConfigureAwait(false);
            await db.Database.MigrateAsync(CancellationToken.None).ConfigureAwait(false);
            await Console.Out.WriteLineAsync("[catalog] done").ConfigureAwait(false);
        }
        else
        {
            await Console.Out.WriteLineAsync("[catalog] already at head").ConfigureAwait(false);
        }
    }

    // ── Step 2 — per-tenant migrations + (optional) seeds ────────────────
    // `seed-demo` short-circuits this: it provisions its own demo tenants inline (Step 3 below).
    if (!cli.CatalogOnly && cli.Command != "seed-demo")
    {
        using var scope = host.Services.CreateScope();
        foreach (var initializer in scope.ServiceProvider.GetServices<IDbInitializer>())
        {
            await initializer.MigrateAsync(CancellationToken.None).ConfigureAwait(false);
        }

        if (cli.SeedAfter)
        {
            foreach (var initializer in scope.ServiceProvider.GetServices<IDbInitializer>())
            {
                await initializer.SeedAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }
    }

    // ── Step 3 — demo seed (verb: `seed-demo`) ───────────────────────────
    // Dev-only: provisions acme + globex with rich demo content; hard-fails outside Development.
    if (cli.Command == "seed-demo")
    {
        var env = host.Services.GetRequiredService<IHostEnvironment>();
        if (!env.IsDevelopment())
        {
            await Console.Error.WriteLineAsync(
                $"[demo-seed] REFUSING to run — DOTNET_ENVIRONMENT is '{env.EnvironmentName}'. "
                + "seed-demo is dev-only by design.")
                .ConfigureAwait(false);
            return 1;
        }

        await Console.Out.WriteLineAsync("[demo-seed] provisioning acme + globex with demo content…")
            .ConfigureAwait(false);
        using var scope = host.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoSeeder>();
        await seeder.RunAsync(CancellationToken.None).ConfigureAwait(false);
        await Console.Out.WriteLineAsync("[demo-seed] done").ConfigureAwait(false);
    }

    await Console.Out.WriteLineAsync("[migrator] finished successfully.").ConfigureAwait(false);
    return 0;
}
#pragma warning disable CA1031 // Top-level Main intentionally catches every exception to convert any failure into exit code 1.
catch (Exception ex)
#pragma warning restore CA1031
{
    logger.LogError(ex, "DbMigrator failed");
    await Console.Out.WriteLineAsync(ex.Message + "\r\n" + ex.InnerException).ConfigureAwait(false);
    await Console.Error.WriteLineAsync($"[migrator] FAILED: {ex.GetType().Name}: {ex.Message}")
        .ConfigureAwait(false);

    if (ex.StackTrace is { } stack)
    {
        await Console.Error.WriteLineAsync(stack).ConfigureAwait(false);
    }
    return 1;
}
finally
{
    // Flush logging buffers + run host shutdown so the operator (and any
    // CI log collector) sees the final lines before the process exits.
    await host.StopAsync().ConfigureAwait(false);
}

static async Task LogConnectionIdentityAsync(string connectionString)
{
    // Best-effort identity probe — never fail the migrator over a logging step.
    try
    {
        await using var conn = new Npgsql.NpgsqlConnection(connectionString);
        await conn.OpenAsync().ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT current_user, current_database()";
        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        if (await reader.ReadAsync().ConfigureAwait(false))
        {
            var role = reader.GetString(0);
            var db = reader.GetString(1);
            await Console.Out.WriteLineAsync(string.Create(
                System.Globalization.CultureInfo.InvariantCulture,
                $"[migrator] connected as role={role} database={db}")).ConfigureAwait(false);
        }
    }
#pragma warning disable CA1031 // Logging-only path: any exception swallowed and reported, never fatal.
    catch (Exception ex)
#pragma warning restore CA1031
    {
        await Console.Out.WriteLineAsync($"[migrator] WARN: could not log connection identity: {ex.Message}")
            .ConfigureAwait(false);
    }
}