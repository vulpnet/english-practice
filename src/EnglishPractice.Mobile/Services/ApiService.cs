using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EnglishPractice.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private string? _accessToken;
    private string? _refreshToken;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public ApiService()
    {
        var baseUrl = Preferences.Get("api_base_url", "https://your-app.onrender.com");
        _http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(100) };
        _accessToken = SecureStorage.Default.GetAsync("access_token").GetAwaiter().GetResult();
        _refreshToken = SecureStorage.Default.GetAsync("refresh_token").GetAwaiter().GetResult();
        if (!string.IsNullOrEmpty(_accessToken))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public bool IsLoggedIn => !string.IsNullOrEmpty(_accessToken);

    public async Task<bool> LoginAsync(string username, string password)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", new { username, password });
        if (!resp.IsSuccessStatusCode) return false;
        return await StoreTokensFromResponse(resp);
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/register", new { username, email, password });
        if (!resp.IsSuccessStatusCode) return false;
        return await StoreTokensFromResponse(resp);
    }

    public async Task<ConversationApiResponse?> SendMessageAsync(string conversationId, string message, string scenario, string level, List<HistoryItem> history)
    {
        var request = new { message, scenario, level, history };
        var resp = await _http.PostAsJsonAsync($"api/conversations/{conversationId}/messages", request);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            if (await RefreshTokenAsync())
                resp = await _http.PostAsJsonAsync($"api/conversations/{conversationId}/messages", request);
        }
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(body).RootElement;
        return JsonSerializer.Deserialize<ConversationApiResponse>(root.GetProperty("data").GetRawText(), _json);
    }

    private async Task<bool> StoreTokensFromResponse(HttpResponseMessage resp)
    {
        var body = await resp.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(body).RootElement;
        var data = root.GetProperty("data");
        _accessToken = data.GetProperty("accessToken").GetString();
        _refreshToken = data.GetProperty("refreshToken").GetString();
        await SecureStorage.Default.SetAsync("access_token", _accessToken ?? string.Empty);
        await SecureStorage.Default.SetAsync("refresh_token", _refreshToken ?? string.Empty);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        return true;
    }

    private async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken)) return false;
        var resp = await _http.PostAsJsonAsync("api/auth/refresh", new { refreshToken = _refreshToken });
        if (!resp.IsSuccessStatusCode) return false;
        return await StoreTokensFromResponse(resp);
    }

    public void Logout()
    {
        SecureStorage.Default.Remove("access_token");
        SecureStorage.Default.Remove("refresh_token");
        _accessToken = null;
        _refreshToken = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }
}

public class ConversationApiResponse
{
    public string Reply { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public string CorrectedText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string NaturalExpression { get; set; } = string.Empty;
}

public class HistoryItem
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
