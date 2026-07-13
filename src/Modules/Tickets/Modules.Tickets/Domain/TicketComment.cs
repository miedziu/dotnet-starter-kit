using FSH.Framework.Core.Domain;

namespace FSH.Modules.Tickets.Domain;

/// <summary>
/// A comment posted on a ticket. Created via Ticket.AddComment so that
/// the parent aggregate stays the consistency boundary — comments are
/// never persisted independently.
/// </summary>
public sealed class TicketComment : BaseEntity<Guid>, ISoftDeletableInt
{
    public Guid TicketId { get; private set; }
    public int? AuthorUserId { get; private set; }
    public string Body { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }

    // Setters are populated by AuditableEntitySaveChangesInterceptor via EF Core's
    // entry.Property(...).CurrentValue — invisible to static analysis.
#pragma warning disable S1144 // EF Core writes these setters via reflection
    public DateTimeOffset? DeletedAt { get; private set; }
    public int? DeletedBy { get; private set; }
#pragma warning restore S1144

    private TicketComment() { }

    internal static TicketComment Create(Guid ticketId, int? authorUserId, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        return new TicketComment
        {
            Id = Guid.CreateVersion7(),
            TicketId = ticketId,
            AuthorUserId = authorUserId,
            Body = body.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}