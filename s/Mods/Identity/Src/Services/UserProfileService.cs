using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Storage;
using FSH.Framework.Storage;
using FSH.Framework.Storage.Services;
using FSH.Framework.Web.Origin;
using FSH.Mods.Identity.Domain;
using FSH.Mods.Identity.Spec.Services;
using FSH.Mods.Identity.Spec.v1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FSH.Mods.Identity.Services;

internal sealed class UserProfileService(
    UserManager<FshUser> userManager,
    SignInManager<FshUser> signInManager,
    IStorageService storageService,
    IOptions<OriginOptions> originOptions,
    IHttpContextAccessor httpContextAccessor) : IUserProfileService
{
    private readonly Uri? _originUrl = originOptions.Value.OriginUrl;

    public async Task<UserDto> GetAsync(string userId, CancellationToken ct)
    {
        // Relies on Finbuckle's tenant filter — callers can only ever read
        // their own user record, which is in the request's resolved tenant.
        var user = await userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(ct);

        _ = user ?? throw new NotFoundException("user not found");

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ImageUrl = ResolveImageUrl(user.ImageUrl),
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            TwoFactorEnabled = user.TwoFactorEnabled,
            DistrictId = user.DistrictId,
            CommuneId = user.CommuneId,
        };
    }

    public Task<int> GetCountAsync(CancellationToken ct) =>
        userManager.Users.AsNoTracking().CountAsync(ct);

    public async Task<List<UserDto>> GetListAsync(CancellationToken ct)
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync(ct);
        var result = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            result.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ImageUrl = ResolveImageUrl(user.ImageUrl),
                IsActive = user.IsActive,
                DistrictId = user.DistrictId,
                CommuneId = user.CommuneId
            });
        }

        return result;
    }

    public async Task UpdateAsync(string userId, string firstName, string lastName, string phoneNumber, FileUploadRequest image, bool deleteCurrentImage, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        _ = user ?? throw new NotFoundException("user not found");

        Uri imageUri = user.ImageUrl ?? null!;
        // image is optional: text-only edits forward a null FileUploadRequest, so guard before
        // dereferencing Data or the common no-image update path NREs.
        if (image?.Data != null)
        {
            var imageString = await storageService.UploadAsync<FshUser>(image, FileType.Image, ct);
            user.ImageUrl = new Uri(imageString, UriKind.RelativeOrAbsolute);
            if (deleteCurrentImage && imageUri != null)
            {
                await storageService.RemoveAsync(imageUri.ToString(), ct);
            }
        }
        else if (deleteCurrentImage && imageUri != null)
        {
            await storageService.RemoveAsync(imageUri.ToString(), ct);
            user.ImageUrl = null;
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        string? currentPhoneNumber = await userManager.GetPhoneNumberAsync(user);
        if (phoneNumber != currentPhoneNumber)
        {
            await userManager.SetPhoneNumberAsync(user, phoneNumber);
        }

        var result = await userManager.UpdateAsync(user);
        await signInManager.RefreshSignInAsync(user);

        if (!result.Succeeded)
        {
            throw new CustomException("Update profile failed");
        }
    }

    public async Task SetImageUrlAsync(string userId, string? imageUrl, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("user not found");

        user.ImageUrl = string.IsNullOrWhiteSpace(imageUrl)
            ? null
            : new Uri(imageUrl, UriKind.RelativeOrAbsolute);

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new CustomException("Update profile image failed");
        }

        await signInManager.RefreshSignInAsync(user);
    }

    public async Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null, CancellationToken ct = default)
    {
        return await userManager.FindByEmailAsync(email.Normalize()) is FshUser user && user.Id != exceptId;
    }

    public async Task<bool> ExistsWithNameAsync(string name, CancellationToken ct = default)
    {
        return await userManager.FindByNameAsync(name) is not null;
    }

    public async Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null, CancellationToken ct = default)
    {
        return await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, ct) is FshUser user && user.Id != exceptId;
    }

    public async Task<int> GetIntIdAsync(string userId, CancellationToken ct = default)
    {
        var intId = await userManager.Users
            .Where(u => u.Id == userId)
            .Select(u => (int?)u.IntId)
            .FirstOrDefaultAsync(ct);

        if (!intId.HasValue)
        {
            throw new NotFoundException($"User with Id '{userId}' not found.");
        }

        return intId.Value;
    }

    public async Task<Guid> GetGuidAsync(int intId, CancellationToken ct = default)
    {
        var userId = await userManager.Users
            .AsNoTracking()
            .Where(u => u.IntId == intId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(userId))
        {
            throw new NotFoundException($"User with IntId '{intId}' not found.");
        }

        return Guid.Parse(userId);
    }

    private string? ResolveImageUrl(Uri? imageUrl)
    {
        if (imageUrl is null)
        {
            return null;
        }

        // Absolute URLs (e.g., S3) pass through unchanged.
        if (imageUrl.IsAbsoluteUri)
        {
            return imageUrl.ToString();
        }

        // For relative paths from local storage, prefix with the API origin and wwwroot.
        if (_originUrl is null)
        {
            var request = httpContextAccessor.HttpContext?.Request;
            if (request is not null && !string.IsNullOrWhiteSpace(request.Scheme) && request.Host.HasValue)
            {
                var baseUri = $"{request.Scheme}://{request.Host.Value}{request.PathBase}";
                var relativePath = imageUrl.ToString().TrimStart('/');
                return $"{baseUri.TrimEnd('/')}/{relativePath}";
            }

            return imageUrl.ToString();
        }

        var originRelativePath = imageUrl.ToString().TrimStart('/');
        return $"{_originUrl.AbsoluteUri.TrimEnd('/')}/{originRelativePath}";
    }
}