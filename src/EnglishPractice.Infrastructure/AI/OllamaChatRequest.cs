using System.Text.Json.Serialization;
namespace EnglishPractice.Infrastructure.AI;

public class OllamaNativeChatRequest
{
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    [JsonPropertyName("messages")] public List<OllamaMessage> Messages { get; set; } = new();
    [JsonPropertyName("stream")] public bool Stream { get; set; } = false;
    [JsonPropertyName("options")] public OllamaModelOptions? Options { get; set; }
}

public class OllamaOpenAIChatRequest
{
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    [JsonPropertyName("messages")] public List<OllamaMessage> Messages { get; set; } = new();
    [JsonPropertyName("stream")] public bool Stream { get; set; } = false;
}

public class OllamaMessage
{
    [JsonPropertyName("role")] public string Role { get; set; } = string.Empty;
    [JsonPropertyName("content")] public string Content { get; set; } = string.Empty;
}

public class OllamaModelOptions
{
    [JsonPropertyName("temperature")] public double Temperature { get; set; } = 0.7;
    [JsonPropertyName("num_predict")] public int NumPredict { get; set; } = 500;
}
