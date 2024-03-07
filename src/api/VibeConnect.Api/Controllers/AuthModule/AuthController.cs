using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VibeConnect.Auth.Module.DTOs;
using VibeConnect.Auth.Module.Services;
using VibeConnect.Shared.Models;
using Swashbuckle.AspNetCore.Annotations;


namespace VibeConnect.Api.Controllers.AuthModule;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
public class AuthController(IAuthService authService) : BaseController
{

    /// <summary>
    /// Register's a new account for user
    /// </summary>
    /// <param name="payload">Register user account request payload</param>
    /// <returns></returns>
    [HttpPost("register")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<RegisterUserResponseDto>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<RegisterUserResponseDto>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<RegisterUserResponseDto>))]
    [SwaggerOperation("Register an account", OperationId = nameof(RegisterAccount))]
    public async Task<IActionResult> RegisterAccount([FromBody] RegisterUserRequestDto payload)
    {
        var response = await authService.RegisterAccount(payload);
        return ToActionResult(response);
    }
    
    /// <summary>
    /// User Login
    /// </summary>
    /// <param name="payload">Login user request payload</param>
    /// <returns></returns>
    [HttpPost("login")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<LoginResponseDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<LoginResponseDto>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<LoginResponseDto>))]
    [SwaggerOperation("Login to the platform", OperationId = nameof(Login))]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto payload)
    {
        var response = await authService.Login(payload);
        return ToActionResult(response);
    }
}