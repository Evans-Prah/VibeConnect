using System.Net;
using Mapster;
using Newtonsoft.Json;
using VibeConnect.Profile.Module.DTOs.Request;
using VibeConnect.Profile.Module.DTOs.Response;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;
using ExternalLink = VibeConnect.Storage.Entities.ExternalLink;

namespace VibeConnect.Profile.Module.Services;

public class ProfileService(IBaseRepository<User> userRepository, ILoggerAdapter<ProfileService> logger) : IProfileService
{
    public async Task<ApiResponse<ProfileResponseDto>> GetUserProfile(string? username)
    {
        try
        {
            logger.LogInformation(
                "Get user profile -> Service: {service} -> Method: {method}. User => {user}",
                nameof(ProfileService),
                nameof(GetUserProfile),
                username
            );

            var user = await userRepository.FindOneAsync(u =>
                u.Username == username && !u.IsSuspended && u.AccountStatus == "Active");

            if (user == null)
            {
                return new ApiResponse<ProfileResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "User does not exist, check username and try again",
                };
            }

            var userProfileDto = user.Adapt<ProfileResponseDto>();

            return new ApiResponse<ProfileResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "User profile retrieved successfully",
                Data = userProfileDto
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Service: {service} --> Method: {method}. An error occurred.", nameof(ProfileService), nameof(GetUserProfile));

            return new ApiResponse<ProfileResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Oops! An error occurred while trying to get user profile. Please try again later."
            };
        }
    }
    
    public async Task<ApiResponse<ProfileResponseDto>> UpdateUserProfile(string? username, UpdateProfileRequestDto updateProfileRequestDto)
    {
        try
        {
            logger.LogInformation(
                "Update user profile -> Service: {service} -> Method: {method}. User => {user}. Payload => {payload}",
                nameof(ProfileService),
                nameof(UpdateUserProfile),
                username,
                JsonConvert.SerializeObject(updateProfileRequestDto)
            );

            var user = await userRepository.FindOneAsync(u =>
                u.Username == username && !u.IsSuspended && u.AccountStatus == "Active");

            if (user == null)
            {
                return new ApiResponse<ProfileResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "User does not exist, check username and try again",
                };
            }

            user.Email = updateProfileRequestDto?.Email ?? user.Email;
            user.FullName = updateProfileRequestDto?.FullName ?? user.FullName;
            user.PhoneNumber = updateProfileRequestDto?.PhoneNumber ?? user.PhoneNumber;
            user.DateOfBirth = updateProfileRequestDto?.DateOfBirth ?? user.DateOfBirth;

            user.Bio = updateProfileRequestDto?.Bio ?? user.Bio;
            user.ProfilePictureUrl = updateProfileRequestDto?.ProfilePictureUrl ?? user.ProfilePictureUrl;
            
            if (updateProfileRequestDto?.DateOfBirth.HasValue ?? false)
            {
                user.DateOfBirth = DateTime.SpecifyKind(updateProfileRequestDto.DateOfBirth.Value.ToUniversalTime(), DateTimeKind.Utc);
            }
            
            if (updateProfileRequestDto?.LanguagePreferences != null)
            {
                user.LanguagePreferences?.RemoveAll(lp => updateProfileRequestDto.LanguagePreferences.Exists(newLp => newLp.Language != lp.Language));
            
                user.LanguagePreferences?.AddRange(updateProfileRequestDto.LanguagePreferences
                    .Where(newLp => !user.LanguagePreferences.Exists(existingLp => existingLp.Language == newLp.Language))
                    .Select(newLp => new LanguagePreference { Language = newLp.Language }));
            }

            
            if (updateProfileRequestDto?.ExternalLinks != null)
            {
                user.ExternalLinks?.RemoveAll(lp => updateProfileRequestDto.ExternalLinks.Exists(newLp => newLp.Name != lp.Name));

                user.ExternalLinks?.AddRange(updateProfileRequestDto.ExternalLinks
                    .Where(newLp => user.ExternalLinks != null && !user.ExternalLinks.Exists(existingLp => existingLp.Name == newLp.Name))
                    .Select(newLp => new ExternalLink { Name = newLp.Name, Url = newLp.Url}));
            }
            
            user.LastActivityDate = DateTimeOffset.UtcNow;
            
            var updateResponse = await userRepository.UpdateAsync(user);
            
            if (updateResponse < 1)
            {
                return new ApiResponse<ProfileResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Something bad happened and it is entirely our fault. Please try again."
                };
            }

            var userProfileDto = user.Adapt<ProfileResponseDto>();

            return new ApiResponse<ProfileResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Your profile has been updated successfully",
                Data = userProfileDto
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Service: {service} --> Method: {method}. An error occurred.", nameof(ProfileService), nameof(UpdateUserProfile));

            return new ApiResponse<ProfileResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Oops! An error occurred while trying to update user profile. Please try again later."
            };
        }
    }
    
    
}