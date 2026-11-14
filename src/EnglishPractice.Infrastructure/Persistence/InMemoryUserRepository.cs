using System.Collections.Concurrent;
using EnglishPractice.Application.Interfaces;
using EnglishPractice.Domain.Entities;

namespace EnglishPractice.Infrastructure.Persistence;

// MVP: in-memory store. Replace with EF Core + PostgreSQL for production multi-user.
public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, User> _users = new();
    private readonly ConcurrentDictionary<string, RefreshToken> _tokens = new();

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => Task.FromResult(_users.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_users.TryGetValue(id, out var u) ? u : null);

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
        => Task.FromResult(_tokens.TryGetValue(token, out var rt) ? rt : null);

    public Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
    {
        _tokens[token.Token] = token;
        return Task.CompletedTask;
    }

    public Task RevokeRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        if (_tokens.TryGetValue(token, out var rt))
            rt.IsRevoked = true;
        return Task.CompletedTask;
    }
}
