using System.Net.Mime;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VibeConnect.Api.Extensions;
using VibeConnect.Post.Module.DTOs.Post;
using VibeConnect.Post.Module.Services.Comment;
using VibeConnect.Post.Module.Services.Post;
using VibeConnect.Post.Module.Services.PostLikes;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;

namespace VibeConnect.Api.Controllers.PostModule;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/comment")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]

public class CommentLikeController(ICommentLikeService commentLikeService) : BaseController
{
    
    /// <summary>
    /// Like a comment
    /// </summary>
    /// <param name="commentId">CommentId to like</param>
    /// <returns>Count of likes for comment</returns>
    [HttpPost("like/{commentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<int>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<int>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<int>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<int>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<int>))]
    [SwaggerOperation("Like a comment", OperationId = nameof(LikeComment))]
    public async Task<IActionResult> LikeComment([FromRoute] string commentId)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await commentLikeService.HandleCommentLike(commentId, currentUser?.Username, true);
        return ToActionResult(response);
    } 
    
    /// <summary>
    /// UnLike a comment
    /// </summary>
    /// <param name="commentId">CommentId to unlike</param>
    /// <returns>Count of likes for comment</returns>
    [HttpDelete("unlike/{commentId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<int>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<int>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<int>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<int>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<int>))]
    [SwaggerOperation("Unlike a comment", OperationId = nameof(UnLikeComment))]
    public async Task<IActionResult> UnLikeComment([FromRoute] string commentId)
    {
        var currentUser = User.GetCurrentUserAccount();
        var response = await commentLikeService.HandleCommentLike(commentId, currentUser?.Username);
        return ToActionResult(response);
    }
    
    /// <summary>
    /// Get users who liked a comment
    /// </summary>
    /// <param name="baseFilter">Filter comments parameters</param>
    /// <param name="commentId">CommentId</param>
    /// <returns>Users who liked a comment</returns>
    [HttpGet("like/{commentId}/users")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>))]
    [SwaggerOperation("Get users who liked a comment", OperationId = nameof(GetUsersWhoLikedComment))]
    public async Task<IActionResult> GetUsersWhoLikedComment([FromRoute] string commentId, [FromQuery] BaseFilter baseFilter)
    {
        var response = await commentLikeService.GetUsersWhoLikedComment(commentId, baseFilter);
        return ToActionResult(response);
    }
    
}