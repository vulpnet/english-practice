namespace EnglishPractice.Infrastructure.AI;
public class OllamaOptions
{
    public const string Section = "Ollama";
    public string BaseUrl { get; set; } = string.Empty;
    public string ChatEndpoint { get; set; } = "/api/chat";
    public string ApiFormat { get; set; } = "vercel"; // native | openai | vercel
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 90;
}
