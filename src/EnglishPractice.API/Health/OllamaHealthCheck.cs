using EnglishPractice.Application.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EnglishPractice.API.Health;

public class OllamaHealthCheck : IHealthCheck
{
    private readonly IOllamaClient _ollama;
    public OllamaHealthCheck(IOllamaClient ollama) => _ollama = ollama;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            var ok = await _ollama.HealthCheckAsync(cts.Token);
            return ok ? HealthCheckResult.Healthy("Ollama reachable") : HealthCheckResult.Unhealthy("Ollama unreachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Ollama health check failed", ex);
        }
    }
}
