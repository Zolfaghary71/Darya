namespace Darya.Infrastructure.TokenService;

public interface IJwtTokenService
{
    string GenerateToken(string username, string role);
}