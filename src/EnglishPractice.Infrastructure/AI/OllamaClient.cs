using System.Net.Http.Json;
using System.Text.Json;
using EnglishPractice.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishPractice.Infrastructure.AI;

public class OllamaClient : IOllamaClient
{
    private readonly HttpClient _http;
    private readonly OllamaOptions _opts;
    private readonly ILogger<OllamaClient> _logger;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public OllamaClient(HttpClient http, IOptions<OllamaOptions> opts, ILogger<OllamaClient> logger)
    {
        _http = http;
        _opts = opts.Value;
        _logger = logger;
    }

    public async Task<string> ChatAsync(string systemPrompt, List<(string role, string content)> messages, CancellationToken ct = default)
    {
        var allMessages = new List<OllamaMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };
        allMessages.AddRange(messages.Select(m => new OllamaMessage { Role = m.role, Content = m.content }));

        return _opts.ApiFormat.ToLower() switch
        {
            "openai" => await ChatOpenAIAsync(_opts.ChatEndpoint, allMessages, ct),
            "vercel" => await ChatVercelAsync(_opts.ChatEndpoint, allMessages, ct),
            _ => await ChatNativeAsync(_opts.ChatEndpoint, allMessages, ct)
        };
    }

    // Format: { messages: [{role, content}] } → { reply: "..." }
    // Used by: https://tools-chatbot.vercel.app/api/chat
    private async Task<string> ChatVercelAsync(string endpoint, List<OllamaMessage> messages, CancellationToken ct)
    {
        var req = new { messages };
        _logger.LogInformation("Calling Vercel chatbot {Endpoint}", endpoint);
        var response = await _http.PostAsJsonAsync(endpoint, req, ct);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return doc.RootElement.GetProperty("reply").GetString()
            ?? throw new InvalidOperationException("No 'reply' field in response");
    }

    private async Task<string> ChatNativeAsync(string endpoint, List<OllamaMessage> messages, CancellationToken ct)
    {
        var req = new OllamaNativeChatRequest
        {
            Model = _opts.Model,
            Messages = messages,
            Stream = false,
            Options = new OllamaModelOptions { Temperature = 0.7, NumPredict = 500 }
        };

        _logger.LogInformation("Calling Ollama native {Endpoint} model={Model}", endpoint, _opts.Model);
        var response = await _http.PostAsJsonAsync(endpoint, req, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaNativeChatResponse>(_json, ct)
            ?? throw new InvalidOperationException("Empty response from Ollama");

        if (!string.IsNullOrEmpty(result.Error))
            throw new InvalidOperationException($"Ollama error: {result.Error}");

        return result.Message?.Content ?? throw new InvalidOperationException("No content in Ollama response");
    }

    private async Task<string> ChatOpenAIAsync(string endpoint, List<OllamaMessage> messages, CancellationToken ct)
    {
        var req = new OllamaOpenAIChatRequest { Model = _opts.Model, Messages = messages, Stream = false };

        _logger.LogInformation("Calling Ollama OpenAI-compat {Endpoint} model={Model}", endpoint, _opts.Model);
        var response = await _http.PostAsJsonAsync(endpoint, req, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaOpenAIChatResponse>(_json, ct)
            ?? throw new InvalidOperationException("Empty response from Ollama");

        if (result.Error != null)
            throw new InvalidOperationException($"Ollama error: {result.Error.Message}");

        return result.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("No choices in Ollama response");
    }

    public async Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            var response = await _http.GetAsync("/", cts.Token);
            return response.IsSuccessStatusCode || (int)response.StatusCode < 500;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama health check failed");
            return false;
        }
    }
}
