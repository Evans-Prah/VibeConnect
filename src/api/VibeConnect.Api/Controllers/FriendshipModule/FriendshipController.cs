using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VibeConnect.Api.Extensions;
using VibeConnect.Friendship.Module.DTOs;
using VibeConnect.Friendship.Module.Services;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;

namespace VibeConnect.Api.Controllers.FriendshipModule;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]
public class FriendshipController(IFriendshipService friendshipService) : BaseController
{
    /// <summary>
    /// Gets all user's followers
    /// </summary>
    /// <param name="username">username.</param>
    /// <param name="baseFilter">Filter sent requests.</param>
    /// <returns>The action result of getting all user followers</returns>
    [HttpGet("followers/{username}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>))]
    [SwaggerOperation("Get all followers of a user", OperationId = nameof(GetUserFollowers))]
    public async Task<IActionResult> GetUserFollowers([FromRoute] string username, [FromQuery] BaseFilter baseFilter)
    {
        var response = await friendshipService.GetUserFollowers(username, baseFilter);

        return ToActionResult(response);
    }
    
    /// <summary>
    /// Gets all user's followings.
    /// </summary>
    /// <param name="username">username.</param>
    /// <param name="baseFilter">Filter sent requests.</param>
    /// <returns>The action result of getting all user following</returns>
    [HttpGet("followings/{username}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>))]
    [SwaggerOperation("Get all followings of a user", OperationId = nameof(GetUserFollowing))]
    public async Task<IActionResult> GetUserFollowing([FromRoute] string username, [FromQuery] BaseFilter baseFilter)
    {
        var response = await friendshipService.GetUserFollowing(username, baseFilter);

        return ToActionResult(response);
    }
    
    /// <summary>
    /// Unfollows a user.
    /// </summary>
    /// <remarks>
    /// This endpoint allows a user to unfollow another user.
    /// </remarks>
    /// <param name="followeeUsername">The username of the user being unfollowed.</param>
    /// <returns>The action result of unfollowing a user.</returns>
    [HttpDelete("unfollow/{followeeUsername}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<bool>))]
    [SwaggerOperation("Unfollow a user", OperationId = nameof(UnfollowUser))]
    public async Task<IActionResult> UnfollowUser([FromRoute] string followeeUsername)
    {
        var user = User.GetCurrentUserAccount();

        var response = await friendshipService.UnfollowUser(user.Username, followeeUsername);

        return ToActionResult(response);
    }
}