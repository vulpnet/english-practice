namespace EnglishPractice.Application.Interfaces;
public interface IOllamaClient
{
    Task<string> ChatAsync(string systemPrompt, List<(string role, string content)> messages, CancellationToken ct = default);
    Task<bool> HealthCheckAsync(CancellationToken ct = default);
}
