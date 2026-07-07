using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Quota;
using System.Collections.Concurrent;

namespace FSH.Framework.Quota;

/// <summary>
/// Per-process quota counter. Suitable for development and tests; not shared across instances so
/// limits are applied independently per host. In multi-node deployments configure Redis instead.
/// </summary>
public sealed class InMemoryQuotaService : IQuotaService
{
    private readonly ConcurrentDictionary<string, long> _counters;
    private readonly QuotaPlanResolver _planResolver;
    private readonly Dictionary<QuotaResource, IQuotaGaugeProvider> _gauges;
    private readonly TimeProvider _timeProvider;

    public InMemoryQuotaService(
        QuotaOptions options,
        QuotaPlanResolver planResolver,
        IEnumerable<IQuotaGaugeProvider> gauges,
        TimeProvider timeProvider,
        IMultiTenantContextAccessor<AppTenantInfo>? tenantAccessor = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(planResolver);
        ArgumentNullException.ThrowIfNull(gauges);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _options = options;
        _planResolver = planResolver;
        _timeProvider = timeProvider;
        _gauges = gauges.ToDictionary(g => g.Resource);
    }

    public ValueTask<QuotaCheckResult> CheckAsync(QuotaResource resource, long amount, CancellationToken ct = default)
    {
        var (limit, exempt) = ResolveLimit(resource);
        var current = GetCounter(resource);

        if (exempt || limit == long.MaxValue)
        {
            return ValueTask.FromResult(QuotaCheckResult.Unlimited(resource, current));
        }

        var allowed = current + amount <= limit;
        return ValueTask.FromResult(new QuotaCheckResult(allowed, resource, current, limit, GetPeriodResetUtc(resource)));
    }

    public ValueTask<long> RecordAsync(QuotaResource resource, long amount, CancellationToken ct = default)
    {
        if (!IsCounterResource(resource))
        {
            return GetCurrentAsync(resource, ct);
        }

        var key = BuildCounterKey(resource);
        var updated = _counters.AddOrUpdate(key, amount, (_, v) => v + amount);
        return ValueTask.FromResult(updated);
    }

    public async ValueTask<QuotaCheckResult> CheckAndRecordAsync(QuotaResource resource, long amount, CancellationToken ct = default)
    {
        var (limit, exempt) = ResolveLimit(resource);

        if (exempt || limit == long.MaxValue)
        {
            var after = await RecordAsync(resource, amount, ct).ConfigureAwait(false);
            return QuotaCheckResult.Unlimited(resource, after);
        }

        if (!IsCounterResource(resource))
        {
            return await CheckAsync(resource, amount, ct).ConfigureAwait(false);
        }

        var key = BuildCounterKey(resource);
        var newValue = _counters.AddOrUpdate(key, amount, (_, v) => v + amount);

        if (newValue <= limit)
        {
            return new QuotaCheckResult(true, resource, newValue, limit, GetPeriodResetUtc(resource));
        }

        _counters.AddOrUpdate(key, 0, (_, v) => v - amount);
        return new QuotaCheckResult(false, resource, newValue - amount, limit, GetPeriodResetUtc(resource));
    }

    public ValueTask<long> GetCurrentAsync(QuotaResource resource, CancellationToken ct = default)
    {
        if (!IsCounterResource(resource))
        {
            if (_gauges.TryGetValue(resource, out var provider))
            {
                return provider.GetCurrentAsync(ct);
            }

            return ValueTask.FromResult(0L);
        }

        return ValueTask.FromResult(GetCounter(resource));
    }

    private long GetCounter(QuotaResource resource)
    {
        return _counters.TryGetValue(BuildCounterKey(resource), out var value) ? value : 0;
    }

    private (long Limit, bool Exempt) ResolveLimit(QuotaResource resource)
    {
        return (_planResolver.ResolveLimit(resource), false);
    }

    private static bool IsCounterResource(QuotaResource resource) => resource switch
    {
        QuotaResource.ApiCalls => true,
        _ => false
    };

    private static bool IsPeriodic(QuotaResource resource) => resource switch
    {
        QuotaResource.ApiCalls => true,
        _ => false
    };

    private string BuildCounterKey(QuotaResource resource)
    {
        if (!IsPeriodic(resource))
        {
            return $"quota:{resource}";
        }

        var now = _timeProvider.GetUtcNow();
        var period = $"{now.Year:D4}{now.Month:D2}";
        return $"quota:{resource}:{period}";
    }

    private DateTimeOffset? GetPeriodResetUtc(QuotaResource resource)
    {
        if (!IsCounterResource(resource))
        {
            return null;
        }

        var now = _timeProvider.GetUtcNow();
        return now.Month == 12
            ? new DateTimeOffset(now.Year + 1, 1, 1, 0, 0, 0, TimeSpan.Zero)
            : new DateTimeOffset(now.Year, now.Month + 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
