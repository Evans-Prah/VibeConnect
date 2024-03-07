using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VibeConnect.Api.Extensions;
using VibeConnect.Post.Module.DTOs.Post;
using VibeConnect.Post.Module.Services.Post;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;

namespace VibeConnect.Api.Controllers.PostModule;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/post/lookup/{username}")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]

public class PostLookupController(IPostService postService) : BaseController
{
    
    /// <summary>
    /// Get a user's posts
    /// </summary>
    /// <param name="username">username to fetch posts</param>
    /// <param name="baseFilter">Filter posts parameters</param>
    /// <returns>Specified User's posts</returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ApiPagedResult<PostResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<ApiPagedResult<PostResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<ApiPagedResult<PostResponseDto>>))]
    [SwaggerOperation("Get all post for a user", OperationId = nameof(GetUserPosts))]
    public async Task<IActionResult> GetUserPosts([FromRoute] string username, [FromQuery] BaseFilter baseFilter)
    {
        var response = await postService.GetUserPosts(baseFilter, username);
        return ToActionResult(response);
    }
    
    
    /// <summary>
    /// Get user post details
    /// </summary>
    /// <param name="username">Username to fetch post details</param>
    /// <param name="postId">PostId to fetch post details</param>
    /// <returns>User's post details</returns>
    [HttpGet("{postId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<PostResponseDto>))]
    [SwaggerOperation("Get a user's post", OperationId = nameof(GetUserPost))]
    public async Task<IActionResult> GetUserPost([FromRoute] string username, [FromRoute] string postId)
    {
        var response = await postService.GetUserPost(postId, username);
        return ToActionResult(response);
    }
}