using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VibeConnect.Api.Extensions;
using VibeConnect.Post.Module.DTOs.Post;
using VibeConnect.Post.Module.Services.Post;
using VibeConnect.Post.Module.Services.PostLikes;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;

namespace VibeConnect.Api.Controllers.PostModule;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/post-like/{postId}")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]

public class PostLikeController(IPostLikeService postLikeService) : BaseController
{
    
    /// <summary>
    /// Like a post
    /// </summary>
    /// <param name="postId">PostId to like</param>
    /// <returns>Count of likes for post</returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<int?>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<int?>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<int?>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<int?>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<int?>))]
    [SwaggerOperation("Like a post", OperationId = nameof(LikePost))]
    public async Task<IActionResult> LikePost([FromRoute] string postId)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await postLikeService.HandlePostLike(postId, currentUser?.Username, true);
        return ToActionResult(response);
    } 
    
    /// <summary>
    /// UnLike a post
    /// </summary>
    /// <param name="postId">PostId to unlike</param>
    /// <returns>Count of likes for post</returns>
    [HttpDelete]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<int?>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<int?>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<int?>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<int?>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<int?>))]
    [SwaggerOperation("Unlike a post", OperationId = nameof(UnLikePost))]
    public async Task<IActionResult> UnLikePost([FromRoute] string postId)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await postLikeService.HandlePostLike(postId, currentUser?.Username);
        return ToActionResult(response);
    }
    
    /// <summary>
    /// Get users who liked a post
    /// </summary>
    /// <param name="baseFilter">Filter posts parameters</param>
    /// <param name="postId">PostId</param>
    /// <returns>Users who liked a post</returns>
    [HttpGet("users")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>))]
    [SwaggerOperation("Get users who liked a post", OperationId = nameof(GetUsersWhoLikedPost))]
    public async Task<IActionResult> GetUsersWhoLikedPost([FromRoute] string postId, [FromQuery] BaseFilter baseFilter)
    {
        var response = await postLikeService.GetUsersWhoLikedPost(postId, baseFilter);
        return ToActionResult(response);
    }
    
}