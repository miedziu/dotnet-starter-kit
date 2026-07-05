using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FSH.Framework.Web.HttpResilience;

public static class Extensions
{
    /// <summary>
    /// Adds a standard resilience handler (retry, circuit breaker, timeout) to the HTTP client builder.
    /// Configuration is read from the "HttpResilienceOptions" section.
    /// </summary>
    public static IHttpClientBuilder AddHeroResilience(this IHttpClientBuilder builder, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration.GetSection(nameof(HttpResilienceOptions)).Get<HttpResilienceOptions>() ?? new HttpResilienceOptions();

        if (!options.Enabled)
        {
            return builder;
        }

        builder.AddStandardResilienceHandler(pipeline =>
        {
            pipeline.Retry.MaxRetryAttempts = options.MaxRetryAttempts;
            pipeline.Retry.Delay = options.MedianFirstRetryDelay;

            pipeline.TotalRequestTimeout.Timeout = options.TotalTimeout;
            pipeline.AttemptTimeout.Timeout = options.AttemptTimeout;

            pipeline.CircuitBreaker.BreakDuration = options.CircuitBreakerBreakDuration;
            pipeline.CircuitBreaker.FailureRatio = options.CircuitBreakerFailureRatio;
            pipeline.CircuitBreaker.MinimumThroughput = options.CircuitBreakerMinimumThroughput;
            // Ensure sampling duration is at least double the attempt timeout to satisfy validators
            var minSampling = TimeSpan.FromTicks(options.AttemptTimeout.Ticks * 2);
            var sampling = options.CircuitBreakerSamplingDuration > minSampling ? options.CircuitBreakerSamplingDuration : minSampling;
            pipeline.CircuitBreaker.SamplingDuration = sampling;
        });

        return builder;
    }
}
