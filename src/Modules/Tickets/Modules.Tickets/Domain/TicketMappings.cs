using FSH.Framework.Core.Exceptions;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Tickets.Contracts.Dtos;

namespace FSH.Modules.Tickets.Domain;

internal static class TicketMappings
{
    public static async ValueTask<TicketDto> ToDto(this Ticket t, IUserProfileService userProfileService, CancellationToken cancellationToken = default) => new(
        t.Id,
        t.Number,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        await GetGuidAsync(t.ReporterUserId, userProfileService, cancellationToken),
        await GetGuidAsync(t.AssignedToUserId, userProfileService, cancellationToken),
        t.ResolutionNote,
        t.CreatedAtUtc,
        t.UpdatedAtUtc,
        t.ResolvedAtUtc,
        t.ClosedAtUtc,
        t.Comments.Count,
        t.DeletedOnUtc,
        (await GetGuidAsync(t.DeletedBy, userProfileService, cancellationToken)).ToString());

    public static async ValueTask<TicketDto> ToDto(this Ticket t, int commentCount, IUserProfileService userProfileService, CancellationToken cancellationToken = default) => new(
        t.Id,
        t.Number,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        await GetGuidAsync(t.ReporterUserId, userProfileService, cancellationToken),
        await GetGuidAsync(t.AssignedToUserId, userProfileService, cancellationToken),
        t.ResolutionNote,
        t.CreatedAtUtc,
        t.UpdatedAtUtc,
        t.ResolvedAtUtc,
        t.ClosedAtUtc,
        commentCount,
        t.DeletedOnUtc,
        (await GetGuidAsync(t.DeletedBy, userProfileService, cancellationToken)).ToString());

    public static async ValueTask<TicketCommentDto> ToDto(this TicketComment c, IUserProfileService userProfileService, CancellationToken cancellationToken = default) => new(
        c.Id, c.TicketId, await GetGuidAsync(c.AuthorUserId, userProfileService, cancellationToken), c.Body, c.CreatedAtUtc);

    private static async ValueTask<Guid> GetGuidAsync(int? intId, IUserProfileService userProfileService, CancellationToken cancellationToken)
    {
        if (!intId.HasValue)
        {
            return Guid.Empty;
        }

        return await userProfileService.GetGuidAsync(intId.Value, cancellationToken).ConfigureAwait(false);
    }
}