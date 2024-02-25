using System.Net;
using Mapster;
using Newtonsoft.Json;
using VibeConnect.Auth.Module.DTOs;
using VibeConnect.Auth.Module.Utilities;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;

namespace VibeConnect.Auth.Module.Services;

public class AuthService(ILoggerAdapter<AuthService> logger, IBaseRepository<User> userRepository) : IAuthService
{
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

            //TODO: Generate a JWT token or perform any other necessary steps for successful login

            return new ApiResponse<LoginResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Login successful.",
                Data = new LoginResponseDto
                {
                    Email = user.Email,
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

}