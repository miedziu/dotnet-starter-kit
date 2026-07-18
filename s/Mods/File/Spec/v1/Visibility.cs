namespace FSH.Mods.File.Spec.v1;

/// <summary>
/// File visibility. Public = visible to anyone in the tenant; Private = uploader-only.
/// Serialized as its string name (global JsonStringEnumConverter), so the SPA sends and
/// receives "Public"/"Private" rather than 0/1. Lives in Spec (not Domain) because it
/// is part of the published wire contract for both commands and Dtos.
/// </summary>
public enum Visibility
{
    Public = 0,
    Private = 1
}