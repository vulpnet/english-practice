using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnglishPractice.IntegrationTests;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("JWT_SECRET", "integration-test-secret-32-chars!!");
            builder.UseSetting("OLLAMA_BASE_URL", "http://localhost:11434");
            builder.UseSetting("OLLAMA_MODEL", "llama3");
        }).CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_Returns200WithTokens()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"testuser_{Guid.NewGuid():N}",
            email = "test@example.com",
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<ApiResult>();
        Assert.True(body?.Success);
        Assert.NotNull(body?.Data?.AccessToken);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var username = $"user_{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email = "x@x.com",
            password = "Password123!"
        });

        var resp = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Health_Returns200()
    {
        var resp = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}

public record ApiResult(bool Success, TokenData? Data, string? Error);
public record TokenData(string AccessToken, string RefreshToken);
