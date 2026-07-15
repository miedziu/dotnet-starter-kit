using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Tickets.Contracts.v1.Dtos;

namespace FSH.Modules.Tickets.Domain;

internal static class TicketMappings
{
    public static async ValueTask<TicketDto> ToDto(this Ticket t, IUserProfileService userProfileService, CancellationToken ct = default) => new(
        t.Id,
        t.Number,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        await GetGuidAsync(t.ReporterUserId, userProfileService, ct),
        await GetGuidAsync(t.AssignedToUserId, userProfileService, ct),
        t.ResolutionNote,
        t.CreatedAtUtc,
        t.UpdatedAtUtc,
        t.ResolvedAtUtc,
        t.ClosedAtUtc,
        t.Comments.Count,
        t.DeletedAt,
        (await GetGuidAsync(t.DeletedBy, userProfileService, ct)).ToString());

    public static async ValueTask<TicketDto> ToDto(this Ticket t, int commentCount, IUserProfileService userProfileService, CancellationToken ct = default) => new(
        t.Id,
        t.Number,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        await GetGuidAsync(t.ReporterUserId, userProfileService, ct),
        await GetGuidAsync(t.AssignedToUserId, userProfileService, ct),
        t.ResolutionNote,
        t.CreatedAtUtc,
        t.UpdatedAtUtc,
        t.ResolvedAtUtc,
        t.ClosedAtUtc,
        commentCount,
        t.DeletedAt,
        (await GetGuidAsync(t.DeletedBy, userProfileService, ct)).ToString());

    public static async ValueTask<TicketCommentDto> ToDto(this TicketComment c, IUserProfileService userProfileService, CancellationToken ct = default) => new(
        c.Id, c.TicketId, await GetGuidAsync(c.AuthorUserId, userProfileService, ct), c.Body, c.CreatedAtUtc);

    private static async ValueTask<Guid> GetGuidAsync(int? intId, IUserProfileService userProfileService, CancellationToken ct)
    {
        if (!intId.HasValue)
        {
            return Guid.Empty;
        }

        return await userProfileService.GetGuidAsync(intId.Value, ct).ConfigureAwait(false);
    }
}