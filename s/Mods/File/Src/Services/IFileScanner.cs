using FSH.Mods.File.Domain;

namespace FSH.Mods.File.Services;

/// <summary>
/// Hook point for antivirus / content scanning. The File module's <c>FinalizeUpload</c> handler
/// calls this after HEAD-ing the freshly uploaded object. Phase A ships a no-op default that always
/// returns <c>Clean</c>; downstream deployments wire ClamAV / GuardDuty / VirusTotal by replacing
/// the registration.
/// </summary>
public interface IFileScanner
{
    ValueTask<ScanStatus> ScanAsync(string storageKey, CancellationToken ct = default);
}