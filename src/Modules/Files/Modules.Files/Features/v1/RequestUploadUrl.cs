using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Files.Contracts.Authorization;
using FSH.Modules.Files.Contracts.v1.Commands;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Storage.Services;
using FSH.Modules.Files.Contracts.v1.DTOs;
using FSH.Modules.Files.Data;
using FSH.Modules.Files.Domain;
using FSH.Modules.Files.Services;
using Microsoft.Extensions.Options;
using System.Net;

namespace FSH.Modules.Files.Features.v1;

public static class RequestUploadUrlEndpoint
{
    internal static RouteHandlerBuilder MapRequestUploadUrlEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/upload-url",
                async (RequestUploadUrlCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("RequestFileUploadUrl")
            .WithSummary("Mint a presigned PUT URL for a file upload")
            .RequirePermission(FilesPermissions.Upload)
            .WithIdempotency();
}

public sealed class RequestUploadUrlCommandValidator : AbstractValidator<RequestUploadUrlCommand>
{
    public RequestUploadUrlCommandValidator()
    {
        RuleFor(x => x.OwnerType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.SizeBytes).GreaterThan(0);
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.Visibility).IsInEnum();
    }
}

public sealed class RequestUploadUrlCommandHandler(
    FilesDbContext db,
    IStorageService storage,
    FileAccessPolicyRegistry policies,
    ICurrentUser currentUser,
    IOptions<FilesOptions> options)
    : ICommandHandler<RequestUploadUrlCommand, PresignedUploadResponse>
{
    public async ValueTask<PresignedUploadResponse> Handle(RequestUploadUrlCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedException("no current user");
        }

        // Category lookup + extension/size validation.
        if (!options.Value.Categories.TryGetValue(cmd.Category, out var category))
        {
            throw new CustomException($"Unknown category '{cmd.Category}'.", (IEnumerable<string>?)null, HttpStatusCode.BadRequest);
        }

        var extension = Path.GetExtension(cmd.FileName);
        if (string.IsNullOrWhiteSpace(extension) ||
            !category.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new CustomException(
                $"Extension '{extension}' not allowed for category '{cmd.Category}'.",
                (IEnumerable<string>?)null,
                HttpStatusCode.BadRequest);
        }

        if (cmd.SizeBytes > category.MaxBytes)
        {
            throw new CustomException(
                $"File exceeds max size of {category.MaxBytes} bytes for category '{cmd.Category}'.",
                (IEnumerable<string>?)null,
                HttpStatusCode.BadRequest);
        }

        // Authorization: policy must exist and allow the attach.
        var policy = policies.Resolve(cmd.OwnerType)
            ?? throw new ForbiddenException($"No file access policy registered for owner type '{cmd.OwnerType}'.");
        if (!await policy.CanAttachAsync(cmd.OwnerId, userId.ToString(), cancellationToken).ConfigureAwait(false))
        {
            throw new ForbiddenException("Not allowed to attach files to this owner.");
        }

        // Generate id + storage key + presigned URL.
        var id = Guid.CreateVersion7();
        var storageKey = StorageKeyBuilder.Build(cmd.OwnerType, id, cmd.FileName, DateTimeOffset.UtcNow);
        var ttl = TimeSpan.FromMinutes(options.Value.UploadUrlTtlMinutes);
        var presigned = await storage.GenerateUploadUrlAsync(storageKey, cmd.ContentType, category.MaxBytes, ttl, cancellationToken).ConfigureAwait(false);

        var asset = FileAsset.CreatePending(
            id: id,
            ownerType: cmd.OwnerType,
            ownerId: cmd.OwnerId,
            originalFileName: cmd.FileName,
            sanitizedFileName: StorageKeyBuilder.Sanitize(cmd.FileName),
            contentType: cmd.ContentType,
            declaredSizeBytes: cmd.SizeBytes,
            storageKey: storageKey,
            visibility: cmd.Visibility,
            createdByUserId: userId.ToString(),
            uploadDeadline: DateTimeOffset.UtcNow.Add(ttl));

        db.FileAssets.Add(asset);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new PresignedUploadResponse(asset.Id, presigned.Url, presigned.RequiredHeaders, presigned.ExpiresAt);
    }
}