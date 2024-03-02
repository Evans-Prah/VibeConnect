using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VibeConnect.Api.Extensions;
using VibeConnect.Profile.Module.DTOs.Request;
using VibeConnect.Profile.Module.DTOs.Response;
using VibeConnect.Profile.Module.Services;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;

namespace VibeConnect.Api.Controllers.ProfileModule;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]

public class ProfileController(IProfileService profileService) : BaseController
{

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>Current user profile</returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<ProfileResponseDto>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<ProfileResponseDto>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<ProfileResponseDto>))]
    [SwaggerOperation(nameof(GetCurrentUserProfile), OperationId = nameof(GetCurrentUserProfile))]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await profileService.GetUserProfile(currentUser?.Username);
        return ToActionResult(response);
    }
    
    /// <summary>
    /// User Profile Update
    /// </summary>
    /// <param name="payload">update user profile request payload</param>
    /// <returns>Updated user profile</returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ProfileResponseDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<ProfileResponseDto>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<ProfileResponseDto>))]
    [SwaggerOperation(nameof(UpdateProfile), OperationId = nameof(UpdateProfile))]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto payload)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await profileService.UpdateUserProfile(currentUser?.Username, payload);
        return ToActionResult(response);
    }
}