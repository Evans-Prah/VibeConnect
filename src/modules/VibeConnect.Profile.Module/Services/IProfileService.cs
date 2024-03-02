using VibeConnect.Profile.Module.DTOs.Request;
using VibeConnect.Profile.Module.DTOs.Response;
using VibeConnect.Shared.Models;

namespace VibeConnect.Profile.Module.Services;

public interface IProfileService
{
    Task<ApiResponse<ProfileResponseDto>> GetUserProfile(string? username);
    Task<ApiResponse<ProfileResponseDto>> UpdateUserProfile(string? username, UpdateProfileRequestDto updateProfileRequestDto);
}