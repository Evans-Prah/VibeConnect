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
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]

public class PostController(IPostService postService) : BaseController
{
    /// <summary>
    /// Create a post by user
    /// </summary>
    /// <param name="postRequestDto">create post payload</param>
    /// <returns>Created post</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<PostResponseDto>))]
    [SwaggerOperation("Create a post", OperationId = nameof(CreatePost))]
    public async Task<IActionResult> CreatePost([FromBody] PostRequestDto postRequestDto)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await postService.CreatePost(currentUser?.Username, postRequestDto);
        return ToActionResult(response);
    } 
    
    /// <summary>
    /// Get current user posts
    /// </summary>
    /// <param name="baseFilter">Filter posts parameters</param>
    /// <returns>Current User's posts</returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ApiPagedResult<PostResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<ApiPagedResult<PostResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<ApiPagedResult<PostResponseDto>>))]
    [SwaggerOperation("Get logged-in user posts", OperationId = nameof(GetCurrentUserPosts))]
    public async Task<IActionResult> GetCurrentUserPosts([FromQuery] BaseFilter baseFilter)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await postService.GetUserPosts(baseFilter, currentUser?.Username);
        return ToActionResult(response);
    }
    
    /// <summary>
    /// Get current user post details
    /// </summary>
    /// <param name="postId">PostId to fetch post details</param>
    /// <returns>Current User's post details</returns>
    [HttpGet("{postId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<PostResponseDto>))]
    [SwaggerOperation("Get logged-in user post details", OperationId = nameof(GetCurrentUserPost))]
    public async Task<IActionResult> GetCurrentUserPost([FromRoute] string postId)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await postService.GetUserPost(postId, currentUser?.Username);
        return ToActionResult(response);
    } 
    
    /// <summary>
    /// Update user's post
    /// </summary>
    /// <param name="postId">Post Id</param>
    /// <param name="postRequestDto">update post payload</param>
    /// <returns>Updated post</returns>
    [HttpPut("{postId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<PostResponseDto>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<PostResponseDto>))]
    [SwaggerOperation("Update a post", OperationId = nameof(UpdatePost))]
    public async Task<IActionResult> UpdatePost([FromRoute] string postId, [FromBody] PostRequestDto postRequestDto)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await postService.UpdatePost(currentUser?.Username, postId, postRequestDto);
        return ToActionResult(response);
    } 
    
    /// <summary>
    /// Delete user's post
    /// </summary>
    /// <param name="postId">Post Id</param>
    /// <returns></returns>
    [HttpDelete("{postId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<bool>))]
    [SwaggerOperation("Delete a post", OperationId = nameof(DeletePost))]
    public async Task<IActionResult> DeletePost([FromRoute] string postId)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await postService.DeletePost(currentUser?.Username, postId);
        return ToActionResult(response);
    } 
}