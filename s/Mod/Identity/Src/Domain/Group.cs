using FSH.Framework.Core.Domain;

namespace FSH.Mod.Identity.Domain;

public class Group : IAuditableEntity, ISoftDeletableInt
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsSystemGroup { get; private set; }

    // IAuditableEntity implementation
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }
    public int? ModifiedBy { get; private set; }

    // ISoftDeletable implementation
    public DateTimeOffset? DeletedAt { get; private set; }
    public int? DeletedBy { get; private set; }

    // Navigation properties
    public virtual ICollection<GroupRole> GroupRoles { get; private set; } = [];
    public virtual ICollection<UserGroup> UserGroups { get; private set; } = [];

    private Group() { } // EF Core

    public static Group Create(string name, string? description = null, bool isDefault = false, bool isSystemGroup = false, string? createdBy = null)
    {
        return new Group
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsDefault = isDefault,
            IsSystemGroup = isSystemGroup,
            CreatedAt = TimeProvider.System.GetUtcNow(),
            CreatedBy = createdBy
        };
    }

    public void Update(string name, string? description, int? modifiedBy = null)
    {
        Name = name;
        Description = description;
        ModifiedAt = TimeProvider.System.GetUtcNow();
        ModifiedBy = modifiedBy;
    }

    public void SetAsDefault(bool isDefault, int? modifiedBy = null)
    {
        IsDefault = isDefault;
        ModifiedAt = TimeProvider.System.GetUtcNow();
        ModifiedBy = modifiedBy;
    }

    public void Delete(int? deletedBy = null)
    {
        DeletedAt = TimeProvider.System.GetUtcNow();
        DeletedBy = deletedBy;
    }
}