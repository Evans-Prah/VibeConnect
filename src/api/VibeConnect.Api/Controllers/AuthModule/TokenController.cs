using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeConnect.Auth.Module.DTOs;
using VibeConnect.Auth.Module.Services;
using VibeConnect.Shared.Models;
using Swashbuckle.AspNetCore.Annotations;
using VibeConnect.Shared;


namespace VibeConnect.Api.Controllers.AuthModule;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
public class TokenController(IAuthService authService) : BaseController
{

    /// <summary>
    /// Generates new token for user
    /// </summary>
    /// <param name="payload">Refresh token request payload</param>
    /// <returns></returns>
    [HttpPost("refresh")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<TokenResponseDto>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<TokenResponseDto>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<TokenResponseDto>))]
    [SwaggerOperation("Request for refresh token", OperationId = nameof(Refresh))]
    public async Task<IActionResult> Refresh([FromBody] TokenRequestDto payload)
    {
        var response = await authService.RefreshToken(payload);
        return ToActionResult(response);
    }
    
    /// <summary>
    /// Revoke token
    /// </summary>
    /// <param name="username">revoke user token param</param>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]
    [HttpPost("revoke/{username}")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<bool>))]
    [SwaggerOperation("Revoke user's refresh token", OperationId = nameof(Revoke))]
    public async Task<IActionResult> Revoke(string username)
    {
        var response = await authService.RevokeRefreshToken(username);
        return ToActionResult(response);
    }
    
}