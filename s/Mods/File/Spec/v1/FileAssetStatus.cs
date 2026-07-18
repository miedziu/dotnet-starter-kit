namespace FSH.Mods.File.Spec.v1;

/// <summary>
/// Upload lifecycle state of a file asset. Serialized as its string name (global
/// JsonStringEnumConverter), so the SPA sees "PendingUpload"/"Available"/"Quarantined".
/// Lives in Spec because it is part of the published wire contract (FileAssetDto).
/// </summary>
public enum FileAssetStatus
{
    PendingUpload = 0,
    Available = 1,
    Quarantined = 2
}