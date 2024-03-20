using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VibeConnect.Api.Extensions;
using VibeConnect.Post.Module.DTOs.Comment;
using VibeConnect.Post.Module.Services.Comment;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;

namespace VibeConnect.Api.Controllers.PostModule;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]

public class CommentController(ICommentService commentService) : BaseController
{
    /// <summary>
    /// Add comment/reply to a post by user
    /// </summary>
    /// <param name="commentRequestDto">Add comment/reply to post payload</param>
    /// <returns>Created post</returns>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<CommentResponseDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<CommentResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<CommentResponseDto>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<CommentResponseDto>))]
    [SwaggerOperation("Comment/reply on a post", OperationId = nameof(AddComment))]
    public async Task<IActionResult> AddComment([FromBody] CommentRequestDto commentRequestDto)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await commentService.AddComment(currentUser?.Username, commentRequestDto);
        return ToActionResult(response);
    } 
    
    /// <summary>
    /// Get comments/replies on post
    /// </summary>
    /// <param name="postId">Post Id</param>
    /// <param name="baseFilter">Filter</param>
    /// <returns>All comments and replies on post</returns>
    [HttpGet("all/{postId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ApiPagedResult<CommentNode>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<ApiPagedResult<CommentNode>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<ApiPagedResult<CommentNode>>))]
    [SwaggerOperation("Get comments/replies on post", OperationId = nameof(GetPostComments))]
    public async Task<IActionResult> GetPostComments([FromRoute] string postId, [FromQuery] BaseFilter baseFilter)
    {
        var response = await commentService.GetPostComments(postId, baseFilter);
        return ToActionResult(response);
    }
    
    /// <summary>
    /// Get comment with replies
    /// </summary>
    /// <param name="id">Comment Id</param>
    /// <returns>Comment and all replies</returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<CommentNode>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<CommentNode>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<CommentNode>))]
    [SwaggerOperation("Get comment with replies", OperationId = nameof(GetCommentWithReplies))]
    public async Task<IActionResult> GetCommentWithReplies([FromRoute] string id)
    {
        var response = await commentService.GetCommentWithReplies(id);
        return ToActionResult(response);
    }
    
    
    /// <summary>
    /// Delete user's comment/reply to a post
    /// </summary>
    /// <param name="commentId">Comment Id</param>
    /// <returns></returns>
    [HttpDelete("{commentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<bool>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<bool>))]
    [SwaggerOperation("Delete user's comment/reply to a post", OperationId = nameof(DeleteComment))]
    public async Task<IActionResult> DeleteComment([FromRoute] string commentId)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await commentService.DeleteComment(commentId, currentUser?.Username);
        return ToActionResult(response);
    } 
}