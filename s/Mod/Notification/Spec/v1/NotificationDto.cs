namespace FSH.Modules.Notifications.Contracts.v1.Dtos;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string? Body,
    string? Link,
    string Source,
    string MetadataJson,
    DateTime? ReadAtUtc,
    DateTime CreatedAtUtc);