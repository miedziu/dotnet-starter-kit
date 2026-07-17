namespace FSH.Framework.Core.Domain;

/// <summary>
/// Marks an entity as supporting soft deletion.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets the UTC timestamp when the entity was deleted. Null if not deleted.
    /// </summary>
    DateTimeOffset? DeletedAt { get; }

    /// <summary>
    /// Gets the identifier of the user who deleted the entity.
    /// </summary>
    string? DeletedBy { get; }
}

public interface ISoftDeletableInt
{
    /// <summary>
    /// Gets the UTC timestamp when the entity was deleted. Null if not deleted.
    /// </summary>
    DateTimeOffset? DeletedAt { get; }

    /// <summary>
    /// Gets the identifier of the user who deleted the entity.
    /// </summary>
    int? DeletedBy { get; }
}