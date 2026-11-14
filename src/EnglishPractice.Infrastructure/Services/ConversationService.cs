using System.Text.Json;
using System.Text.RegularExpressions;
using EnglishPractice.Application.DTOs.Conversation;
using EnglishPractice.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnglishPractice.Infrastructure.Services;

public class ConversationService : IConversationService
{
    private readonly IOllamaClient _ollama;
    private readonly ILogger<ConversationService> _logger;
    private const int MaxHistoryMessages = 20;

    public ConversationService(IOllamaClient ollama, ILogger<ConversationService> logger)
    {
        _ollama = ollama;
        _logger = logger;
    }

    public async Task<ConversationResponse> SendMessageAsync(string userId, SendMessageRequest request, CancellationToken ct = default)
    {
        var systemPrompt = BuildSystemPrompt(request.Scenario, request.Level);
        var history = request.History
            .TakeLast(MaxHistoryMessages)
            .Select(m => (m.Role.ToLower(), m.Content))
            .ToList();
        history.Add(("user", request.Message));

        string rawResponse;
        try
        {
            rawResponse = await _ollama.ChatAsync(systemPrompt, history, ct);
            _logger.LogInformation("Ollama responded for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama call failed for user {UserId}", userId);
            throw;
        }

        return ParseResponse(rawResponse, request.Message);
    }

    private static string BuildSystemPrompt(string scenario, string level) => $"""
        You are a native English-speaking conversation partner helping someone practice {scenario} conversations.
        The user's English level is {level}.

        Rules:
        1. Always respond naturally in English.
        2. Keep the conversation engaging and contextually appropriate for {scenario}.
        3. Correct grammar and vocabulary mistakes gently.
        4. Explain mistakes briefly in Vietnamese (1-2 sentences max).
        5. Suggest more natural English expressions when appropriate.
        6. Keep responses concise — no long explanations.
        7. Always continue the conversation after corrections.
        8. Adapt vocabulary complexity to {level} level.

        IMPORTANT: You MUST respond ONLY with valid JSON in this exact format:
        {{
          "reply": "Your conversational response here",
          "originalText": "The user's original text",
          "correctedText": "The corrected version (same as original if no errors)",
          "explanation": "Brief explanation in Vietnamese (empty string if no errors)",
          "naturalExpression": "A more natural way to say it (empty string if already natural)"
        }}

        Do not include any text outside the JSON object.
        """;

    private static ConversationResponse ParseResponse(string raw, string originalMessage)
    {
        var jsonMatch = Regex.Match(raw, @"\{[\s\S]*\}", RegexOptions.Multiline);
        var jsonStr = jsonMatch.Success ? jsonMatch.Value : raw;

        try
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<ConversationResponse>(jsonStr, opts);
            if (parsed != null && !string.IsNullOrEmpty(parsed.Reply))
                return parsed;
        }
        catch { }

        return new ConversationResponse
        {
            Reply = raw.Length > 1000 ? raw[..1000] : raw,
            OriginalText = originalMessage,
            CorrectedText = originalMessage,
            Explanation = string.Empty,
            NaturalExpression = string.Empty
        };
    }
}
