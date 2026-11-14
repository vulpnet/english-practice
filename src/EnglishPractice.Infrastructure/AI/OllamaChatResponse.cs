using System.Text.Json.Serialization;
namespace EnglishPractice.Infrastructure.AI;

public class OllamaNativeChatResponse
{
    [JsonPropertyName("message")] public OllamaMessage? Message { get; set; }
    [JsonPropertyName("done")] public bool Done { get; set; }
    [JsonPropertyName("error")] public string? Error { get; set; }
}

public class OllamaOpenAIChatResponse
{
    [JsonPropertyName("choices")] public List<OpenAIChoice>? Choices { get; set; }
    [JsonPropertyName("error")] public OpenAIError? Error { get; set; }
}

public class OpenAIChoice
{
    [JsonPropertyName("message")] public OllamaMessage? Message { get; set; }
}

public class OpenAIError
{
    [JsonPropertyName("message")] public string? Message { get; set; }
}
