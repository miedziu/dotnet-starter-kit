// Add this hosted service class once in your audit module
using FSH.Mods.Audit.Spec;
using Microsoft.Extensions.Hosting;

namespace FSH.Mods.Audit.Core;

public sealed class AuditConfigurator : IHostedService
{
    private readonly IAuditPublisher _publisher;
    private readonly IAuditSerializer _serializer;
    private readonly IEnumerable<IAuditEnricher> _enrichers;

    public AuditConfigurator(
        IAuditPublisher publisher,
        IAuditSerializer serializer,
        IEnumerable<IAuditEnricher> enrichers)
    {
        _publisher = publisher;
        _serializer = serializer;
        _enrichers = enrichers;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Audit.Configure(_publisher, _serializer, _enrichers);
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}