using EnglishPractice.Application.DTOs.Auth;
using EnglishPractice.Application.Interfaces;
using EnglishPractice.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EnglishPractice.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository users, IJwtService jwt, ILogger<AuthService> logger)
    {
        _users = users;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _users.GetByUsernameAsync(request.Username, ct);
        if (existing != null) throw new InvalidOperationException("Username already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };
        await _users.AddAsync(user, ct);
        _logger.LogInformation("User registered: {Username}", user.Username);
        return await CreateTokensAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByUsernameAsync(request.Username, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        _logger.LogInformation("User logged in: {Username}", user.Username);
        return await CreateTokensAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var rt = await _users.GetRefreshTokenAsync(request.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        if (rt.IsRevoked || rt.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired");

        await _users.RevokeRefreshTokenAsync(request.RefreshToken, ct);
        var user = await _users.GetByIdAsync(rt.UserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        return await CreateTokensAsync(user, ct);
    }

    private async Task<AuthResponse> CreateTokensAsync(User user, CancellationToken ct)
    {
        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();
        await _users.AddRefreshTokenAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        }, ct);
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Username = user.Username
        };
    }
}
