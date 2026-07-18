using FSH.Framework.Core.Domain;
using FSH.Mod.Billing.Spec;

namespace FSH.Mod.Billing.Domain;

public sealed class TopupRequest : AggregateRoot<Guid>
{
    public Money Amount { get; private set; } = default!;
    public string? Note { get; private set; }
    public TopupRequestStatus Status { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public string? RequestedBy { get; private set; }
    public string? DecisionNote { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DecidedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    private TopupRequest() { }

    public static TopupRequest Create(decimal amount, string currency, string? note, string? requestedBy)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(amount, 0m);
        return new TopupRequest
        {
            Id = Guid.CreateVersion7(),
            Amount = new Money(amount, string.IsNullOrWhiteSpace(currency) ? "USD" : currency),
            Note = note,
            RequestedBy = requestedBy,
            Status = TopupRequestStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkInvoiced(Guid invoiceId, string? note)
    {
        Require(TopupRequestStatus.Pending);
        InvoiceId = invoiceId;
        DecisionNote = note;
        Status = TopupRequestStatus.Invoiced;
        DecidedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        Require(TopupRequestStatus.Invoiced);
        Status = TopupRequestStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void Reject(string? reason)
    {
        Require(TopupRequestStatus.Pending);
        DecisionNote = reason;
        Status = TopupRequestStatus.Rejected;
        DecidedAtUtc = DateTime.UtcNow;
    }

    private void Require(TopupRequestStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Top-up request must be {expected} (was {Status}).");
    }
}