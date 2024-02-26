using System.Security.Claims;

namespace VibeConnect.Auth.Module.Services;

public interface ITokenService
{
    string GenerateAccessToken(string username);
    string GenerateRefreshToken();
    (bool Success, ClaimsPrincipal? Principal, string ErrorMessage) GetPrincipalFromExpiredToken(string token);
}