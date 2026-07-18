using Asp.Versioning;
using FluentValidation;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Identity;
using FSH.Framework.Web.Modules;
using FSH.Mods.File.Authorization;
using FSH.Mods.File.Data;
using FSH.Mods.File.Features.v1;
using FSH.Mods.File.Jobs;
using FSH.Mods.File.Services;
using FSH.Mods.File.Spec;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

[assembly: FshModule(typeof(FSH.Mods.File.FileModule), 350)]

namespace FSH.Mods.File;

/// <summary>
/// File module: presigned-URL file lifecycle (upload, finalize, serve, delete) shared across the
/// kit's owning features (Ticket attachments, My Files, avatars.
/// Module order 350 places it between Audit (300) and Webhook (400); owning modules
/// (Ticket=700) load later and register their <see cref="IFileAccessPolicy"/>
/// implementations during their own ConfigureServices.
/// </summary>
public sealed class FileModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PermissionConstants.Register(FilePermissions.All);

        builder.Services.Configure<FileOptions>(builder.Configuration.GetSection("Files"));
        builder.Services.AddHeroDbContext<FileDbContext>();
        builder.Services.AddScoped<IDbInitializer, FileDbInitializer>();

        builder.Services.AddScoped<FileAccessPolicyRegistry>();
        builder.Services.AddSingleton<IFileScanner, NoOpFileScanner>();
        builder.Services.AddValidatorsFromAssembly(typeof(FileModule).Assembly);

        // Default uploader-only policies for the built-in OwnerTypes. Owning modules register their
        // own policies for additional OwnerTypes via services.AddFileAccessPolicy<TPolicy>().
        builder.Services.AddScoped<IFileAccessPolicy>(_ => new DefaultUploaderOnlyPolicy("MyFiles"));
        builder.Services.AddScoped<IFileAccessPolicy>(_ => new DefaultUploaderOnlyPolicy("User"));

        builder.Services.AddHealthChecks().AddDbContextCheck<FileDbContext>(
            name: "db:files",
            failureStatus: HealthStatus.Unhealthy);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/files")
            .WithTags("Files")
            .WithApiVersionSet(versionSet)
            .RequireAuthorization();

        // Literal routes first so they win over the /{id:guid} catch-all (matches the Catalog
        // pattern for /trash etc.).
        group.MapRequestUploadUrlEndpoint();         // POST  /upload-url
        group.MapListMyFilesEndpoint();              // GET   /mine
        group.MapListSharedFilesEndpoint();          // GET   /shared
        group.MapListTrashedFilesEndpoint();         // GET   /trash
        group.MapRestoreFileEndpoint();              // POST  /{id}/restore  (literal verb path)

        group.MapFinalizeUploadEndpoint();           // POST  /{id}/finalize
        group.MapGetFileDownloadUrlEndpoint();       // GET   /{id}/url
        group.MapChangeFileVisibilityEndpoint();     // PATCH /{id}/visibility
        group.MapGetFileMetadataEndpoint();          // GET   /{id}
        group.MapDeleteFileEndpoint();               // DELETE /{id}

        // Recurring Hangfire jobs (orphan + retention purges). Registration here matches the
        // pattern Billing uses for MonthlyInvoiceJob.
        var jobManager = endpoints.ServiceProvider.GetService<IRecurringJobManager>();
        if (jobManager is not null)
        {
            jobManager.AddOrUpdate<PurgeOrphanedFilesJob>(
                "files-purge-orphans",
                j => j.RunAsync(CancellationToken.None),
                "0 * * * *", // hourly
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            jobManager.AddOrUpdate<PurgeDeletedFilesJob>(
                "files-purge-deleted",
                j => j.RunAsync(CancellationToken.None),
                "30 3 * * *", // daily 03:30 UTC
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
        }
    }
}