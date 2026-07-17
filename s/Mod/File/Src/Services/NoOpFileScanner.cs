using FSH.Modules.Files.Domain;

namespace FSH.Modules.Files.Services;

internal sealed class NoOpFileScanner : IFileScanner
{
    public ValueTask<ScanStatus> ScanAsync(string storageKey, CancellationToken ct = default)
        => ValueTask.FromResult(ScanStatus.Clean);
}