using VibeConnect.Auth.Module.DTOs;
using VibeConnect.Shared.Models;

namespace VibeConnect.Auth.Module.Services;

public interface IAuthService
{
    Task<ApiResponse<RegisterUserResponseDto>> RegisterAccount(
        RegisterUserRequestDto registerUserRequestDto);

    Task<ApiResponse<LoginResponseDto>> Login(LoginRequestDto loginRequestDto);
    Task<ApiResponse<TokenResponseDto>> RefreshToken(TokenRequestDto tokenRequestDto);
    Task<ApiResponse<bool>> RevokeRefreshToken(string username);
}