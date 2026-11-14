using System.Net.Http.Headers;
using System.Text;
using EnglishPractice.Application.Interfaces;
using EnglishPractice.Infrastructure.AI;
using EnglishPractice.Infrastructure.Persistence;
using EnglishPractice.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishPractice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<OllamaOptions>(opts =>
        {
            opts.BaseUrl = config["OLLAMA_BASE_URL"] ?? config["Ollama:BaseUrl"] ?? "https://tools-chatbot.vercel.app";
            opts.ChatEndpoint = config["OLLAMA_CHAT_ENDPOINT"] ?? config["Ollama:ChatEndpoint"] ?? "/api/chat";
            opts.ApiFormat = config["OLLAMA_API_FORMAT"] ?? config["Ollama:ApiFormat"] ?? "vercel";
            opts.Username = config["OLLAMA_USERNAME"] ?? config["Ollama:Username"] ?? string.Empty;
            opts.Password = config["OLLAMA_PASSWORD"] ?? config["Ollama:Password"] ?? string.Empty;
            opts.Model = config["OLLAMA_MODEL"] ?? config["Ollama:Model"] ?? string.Empty;
            opts.TimeoutSeconds = int.TryParse(config["OLLAMA_TIMEOUT_SECONDS"] ?? config["Ollama:TimeoutSeconds"], out var t) ? t : 30;
        });

        services.AddHttpClient<IOllamaClient, OllamaClient>((sp, client) =>
        {
            var baseUrl = config["OLLAMA_BASE_URL"] ?? config["Ollama:BaseUrl"] ?? "https://tools-chatbot.vercel.app";
            var username = config["OLLAMA_USERNAME"] ?? config["Ollama:Username"] ?? string.Empty;
            var password = config["OLLAMA_PASSWORD"] ?? config["Ollama:Password"] ?? string.Empty;
            var timeout = int.TryParse(config["OLLAMA_TIMEOUT_SECONDS"] ?? config["Ollama:TimeoutSeconds"], out var t) ? t : 30;

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeout);

            if (!string.IsNullOrEmpty(username))
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }
        });

        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IConversationService, ConversationService>();

        return services;
    }
}
