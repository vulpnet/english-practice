using EnglishPractice.Domain.Entities;
namespace EnglishPractice.Application.Interfaces;
public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}
