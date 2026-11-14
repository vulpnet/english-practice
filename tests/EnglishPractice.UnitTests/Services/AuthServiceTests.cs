using EnglishPractice.Application.DTOs.Auth;
using EnglishPractice.Application.Interfaces;
using EnglishPractice.Domain.Entities;
using EnglishPractice.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EnglishPractice.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IJwtService> _jwtMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _jwtMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("access_token");
        _jwtMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");
        _repoMock.Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _sut = new AuthService(_repoMock.Object, _jwtMock.Object, NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsTokens()
    {
        _repoMock.Setup(x => x.GetByUsernameAsync("testuser", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _repoMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.RegisterAsync(new RegisterRequest { Username = "testuser", Email = "t@t.com", Password = "Password123!" });

        Assert.Equal("access_token", result.AccessToken);
        Assert.Equal("refresh_token", result.RefreshToken);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task RegisterAsync_ExistingUser_ThrowsInvalidOperation()
    {
        _repoMock.Setup(x => x.GetByUsernameAsync("existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Username = "existing" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.RegisterAsync(new RegisterRequest { Username = "existing", Email = "e@e.com", Password = "Password123!" }));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        _repoMock.Setup(x => x.GetByUsernameAsync("user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Username = "user", PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct") });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.LoginAsync(new LoginRequest { Username = "user", Password = "wrong" }));
    }

    [Fact]
    public async Task LoginAsync_CorrectPassword_ReturnsTokens()
    {
        _repoMock.Setup(x => x.GetByUsernameAsync("user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Username = "user", PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct") });

        var result = await _sut.LoginAsync(new LoginRequest { Username = "user", Password = "correct" });

        Assert.Equal("access_token", result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorized()
    {
        _repoMock.Setup(x => x.GetByUsernameAsync("nobody", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.LoginAsync(new LoginRequest { Username = "nobody", Password = "pass" }));
    }
}
