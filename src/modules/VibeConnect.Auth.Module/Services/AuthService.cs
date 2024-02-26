using System.Net;
using Mapster;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VibeConnect.Auth.Module.DTOs;
using VibeConnect.Auth.Module.Options;
using VibeConnect.Auth.Module.Utilities;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;

namespace VibeConnect.Auth.Module.Services;

public class AuthService(ILoggerAdapter<AuthService> logger, IBaseRepository<User> userRepository, 
    ITokenService tokenService, IOptions<JwtConfig> jwtOptions) : IAuthService
{
    private readonly JwtConfig _jwtOptions = jwtOptions.Value;

    public async Task<ApiResponse<RegisterUserResponseDto>> RegisterAccount(
        RegisterUserRequestDto registerUserRequestDto)
    {
        try
        {
            logger.LogInformation(
                "Register user -> Service: {service} -> Method: {method}. RequestPayload => {RequestPayload}",
                nameof(AuthService),
                nameof(RegisterAccount),
                JsonConvert.SerializeObject(registerUserRequestDto)
            );

            var existingUser = await userRepository.FindOneAsync(u =>
                u.Email == registerUserRequestDto.Email || u.Username == registerUserRequestDto.Username);

            if (existingUser != null)
            {
                if (existingUser.Email == registerUserRequestDto.Email)
                {
                    return new ApiResponse<RegisterUserResponseDto>
                    {
                        ResponseCode = (int)HttpStatusCode.Conflict,
                        Message = "Email is already registered. Please use a different email address."
                    };
                }

                if (existingUser.Username == registerUserRequestDto.Username)
                {
                    return new ApiResponse<RegisterUserResponseDto>
                    {
                        ResponseCode = (int)HttpStatusCode.Conflict,
                        Message = "Username is already taken. Please choose a different username."
                    };
                }
            }
            
            var hashPassword = PasswordHelper.HashPassword(registerUserRequestDto.Password, out var salt);
            
            var newUser = new User
            {
                Email = registerUserRequestDto.Email,
                Username = registerUserRequestDto.Username,
                PasswordHash = Convert.FromHexString(hashPassword),
                Salt = salt,
                PhoneNumber = registerUserRequestDto.PhoneNumber,
                FullName = registerUserRequestDto.FullName,
                DateOfBirth = DateTime.SpecifyKind(registerUserRequestDto.DateOfBirth, DateTimeKind.Utc),
                Bio = registerUserRequestDto.Bio,
                ProfilePictureUrl = registerUserRequestDto.ProfilePictureUrl
            };

            var response = await userRepository.AddAsync(newUser);

            if (response < 1)
            {
                return new ApiResponse<RegisterUserResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "We are unable to create the account at this moment. Please try again."
                };
            }

            var newUserResponseDto = newUser.Adapt<RegisterUserResponseDto>();
            
            return new ApiResponse<RegisterUserResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.Created,
                Message = "Account created successfully.",
                Data = newUserResponseDto
            };

        }
        catch (Exception e)
        {
            logger.LogError(e, "Service: {service} --> Method: {method}. An error occurred.", nameof(AuthService), nameof(RegisterAccount));
            return new ApiResponse<RegisterUserResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Oops! We could not create the account. It is our fault, try again in few minutes"
            };
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> Login(LoginRequestDto loginRequestDto)
    {
        try
        {
            logger.LogInformation(
                "Login user -> Service: {service} -> Method: {method}. RequestPayload => {RequestPayload}",
                nameof(AuthService),
                nameof(Login),
                JsonConvert.SerializeObject(loginRequestDto)
            );

            var user = await userRepository.FindOneAsync(u =>
                u.Email == loginRequestDto.Email || u.Username == loginRequestDto.Username);
            
            if (user == null)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Invalid credentials"
                };
            }
            
            var verifyPassword = PasswordHelper.VerifyPassword(loginRequestDto.Password, user.PasswordHash, user.Salt);

            if (!verifyPassword)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Invalid credentials"
                };
            }
            
            var accessToken = tokenService.GenerateAccessToken(user.Username);
            var refreshToken = tokenService.GenerateRefreshToken();

            var updateTokenResponse = await UpdateUserRefreshToken(user, refreshToken);

            if (!updateTokenResponse)
            {
                return new ApiResponse<LoginResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Something bad happened and it is entirely our fault. Please try again."
                };
            }
            
            return new ApiResponse<LoginResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Login successful.",
                Data = new LoginResponseDto
                {
                    Email = user.Email,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Service: {service} --> Method: {method}. An error occurred.", nameof(AuthService), nameof(Login));
            return new ApiResponse<LoginResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Oops! An error occurred while trying to log in. Please try again later."
            };
        }
    }

    public async Task<ApiResponse<TokenResponseDto>> RefreshToken(TokenRequestDto tokenRequestDto)
    {
        try
        {
            logger.LogInformation(
                "Generate new tokens for user -> Service: {service} -> Method: {method}. RequestPayload => {RequestPayload}",
                nameof(AuthService),
                nameof(RefreshToken),
                JsonConvert.SerializeObject(tokenRequestDto)
            );
            
            var principal = tokenService.GetPrincipalFromExpiredToken(tokenRequestDto.AccessToken);
            if (!principal.Success)
            {
                return new ApiResponse<TokenResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.BadRequest,
                    Message = principal.ErrorMessage ?? "Invalid access token or refresh token"
                }; 
            }
            
            var username = principal.Principal?.Identity?.Name;
            
            var user = await userRepository.FindOneAsync(u => u.Username == username);

            if (user is null || user.RefreshToken != tokenRequestDto.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
            {
                return new ApiResponse<TokenResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid access token or refresh token"
                };
            }

            var newAccessToken = tokenService.GenerateAccessToken(user.Username);
            var newRefreshToken = tokenService.GenerateRefreshToken();
            
            var updateTokenResponse = await UpdateUserRefreshToken(user, newRefreshToken);

            if (!updateTokenResponse)
            {
                return new ApiResponse<TokenResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Something bad happened and it is entirely our fault. Please try again."
                };
            }

            return new ApiResponse<TokenResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "New tokens generated successfully",
                Data = new TokenResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                }
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Service: {service} --> Method: {method}. An error occurred.", nameof(AuthService), nameof(RefreshToken));
            return new ApiResponse<TokenResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Oops! An error occurred while trying to get new tokens. Please try again later."
            };
        }
    }

    public async Task<ApiResponse<bool>> RevokeRefreshToken(string username)
    {
        try
        {
            logger.LogInformation(
                "Received request to revoke user refresh token -> Service: {service} -> Method: {method}. User => {username}",
                nameof(AuthService),
                nameof(RevokeRefreshToken),
                username
            );
            
            var user = await userRepository.FindOneAsync(u => u.Username == username);

            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid user name"
                };
            }

            user.RefreshToken = null;
            var updateResponse = await userRepository.UpdateAsync(user);
            if (updateResponse < 1)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Something bad happened and it is entirely our fault. Please try again."
                };
            }
            
            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Refresh token revoked successfully."
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "[RevokeRefreshToken]: An error occurred.");
            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Oops! An error occurred while trying to revoke token. Please try again later."
            };
        }
    }
    

    private async Task<bool> UpdateUserRefreshToken(User user, string newRefreshToken)
    {
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenAddedAt = DateTimeOffset.UtcNow;
        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.RefreshTokenValidityPeriod);
        user.LastLoginDate = DateTimeOffset.UtcNow; 
            
        var updateResponse = await userRepository.UpdateAsync(user);

        return updateResponse >= 1;
    }
}