using FSH.Framework.Core.Domain;
using FSH.Mods.Billing.Spec;

namespace FSH.Mods.Billing.Domain;

public sealed class WalletTransaction : BaseEntity<Guid>
{
    public Guid WalletId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public WalletTransactionKind Kind { get; private set; }
    public string Description { get; private set; } = default!;
    public string? ReferenceId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private WalletTransaction() { }

    internal static WalletTransaction Create(
        Guid walletId, Money amount,
        WalletTransactionKind kind, string description, string? referenceId)
        => new()
        {
            Id = Guid.CreateVersion7(),
            WalletId = walletId,
            Amount = amount,
            Kind = kind,
            Description = description,
            ReferenceId = referenceId,
            CreatedAtUtc = DateTime.UtcNow
        };
}