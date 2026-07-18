using FSH.Mod.File.Domain;

namespace FSH.Mod.File.Services;

internal sealed class NoOpFileScanner : IFileScanner
{
    public ValueTask<ScanStatus> ScanAsync(string storageKey, CancellationToken ct = default)
        => ValueTask.FromResult(ScanStatus.Clean);
}