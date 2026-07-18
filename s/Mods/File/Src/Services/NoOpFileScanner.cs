using FSH.Mods.File.Domain;

namespace FSH.Mods.File.Services;

internal sealed class NoOpFileScanner : IFileScanner
{
    public ValueTask<ScanStatus> ScanAsync(string storageKey, CancellationToken ct = default)
        => ValueTask.FromResult(ScanStatus.Clean);
}