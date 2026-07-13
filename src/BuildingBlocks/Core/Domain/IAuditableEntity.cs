namespace FSH.Framework.Core.Domain;

/// <summary>
/// Defines audit metadata for an entity.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets the UTC timestamp when the entity was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the identifier of the creator.
    /// </summary>
    string? CreatedBy { get; }

    /// <summary>
    /// Gets the UTC timestamp when the entity was last modified.
    /// </summary>
    DateTimeOffset? LastModifiedAt { get; }

    /// <summary>
    /// Gets the identifier of the last modifier.
    /// </summary>
    int? LastModifiedBy { get; }
}