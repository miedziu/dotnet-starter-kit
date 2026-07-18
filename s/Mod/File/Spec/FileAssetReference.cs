namespace FSH.Mod.File.Spec;

/// <summary>Owning-feature handle to a FileAsset. Stored on join tables in Ticket/etc.</summary>
public sealed record FileAssetReference(Guid Id, string OwnerType, Guid? OwnerId);