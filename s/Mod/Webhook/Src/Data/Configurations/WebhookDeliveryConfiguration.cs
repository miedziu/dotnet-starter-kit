using FSH.Mod.Webhook.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Mod.Webhook.Data.Configurations;

public sealed class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("Deliveries", "webhooks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(4096);
        builder.HasIndex(x => x.SubscriptionId);
        builder.HasIndex(x => x.AttemptedAtUtc);
    }
}