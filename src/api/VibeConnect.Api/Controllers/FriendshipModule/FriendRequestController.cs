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
[Route("api/v{version:apiVersion}/friend-requests")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize(AuthenticationSchemes = $"{CommonConstants.AuthScheme.Bearer}")]

public class FriendRequestController(IFriendRequestService friendRequestService) : BaseController
{
    /// <summary>
    /// Gets all friend requests.
    /// </summary>
    /// <param name="baseFilter">Filter friend requests.</param>
    /// <param name="sentRequests">Filter sent requests.</param>
    /// <returns>The action result of getting all friend requests.</returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ApiPagedResult<FriendRequestResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<ApiPagedResult<FriendRequestResponseDto>>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<ApiPagedResult<FriendRequestResponseDto>>))]
    [SwaggerOperation("Get all friend requests", OperationId = nameof(GetAllFriendRequests))]
    public async Task<IActionResult> GetAllFriendRequests([FromQuery] BaseFilter baseFilter, [FromQuery] bool sentRequests)
    {
        var user = User.GetCurrentUserAccount();
        var response = await friendRequestService.GetFriendRequests(user?.Username, baseFilter, sentRequests);

        return ToActionResult(response);
    }
    
    /// <summary>
    /// Sends a friend request.
    /// </summary>
    /// <param name="payload">The friend request payload.</param>
    /// <returns>The action result of sending the friend request.</returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<object>))]
    [SwaggerOperation("Send friend request", OperationId = nameof(SendFriendRequest))]
    public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto payload)
    {
        var user = User.GetCurrentUserAccount();
        var response = await friendRequestService.SendFriendRequest(user?.Username, payload);

        return ToActionResult(response);
    }

    /// <summary>
    /// Accepts a friend request.
    /// </summary>
    /// <param name="requestId">The ID of the friend request.</param>
    /// <returns>The action result of accepting the friend request.</returns>
    [HttpPatch("accept-friend-request/{requestId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<object>))]
    [SwaggerOperation("Accept friend request", OperationId = nameof(AcceptFriendRequest))]
    public async Task<IActionResult> AcceptFriendRequest([FromRoute] string requestId)
    {
        var user = User.GetCurrentUserAccount();
        var response = await friendRequestService.ApproveFriendRequest(requestId, user?.Username);

        return ToActionResult(response);
    }
    
    /// <summary>
    /// Rejects a friend request.
    /// </summary>
    /// <param name="requestId">The ID of the friend request.</param>
    /// <returns>The action result of rejecting the friend request.</returns>
    [HttpPatch("reject-friend-request/{requestId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(ApiResponse<object>))]
    [SwaggerOperation("Reject friend request", OperationId = nameof(RejectFriendRequest))]
    public async Task<IActionResult> RejectFriendRequest([FromRoute] string requestId)
    {
        var user = User.GetCurrentUserAccount();
        var response = await friendRequestService.RejectFriendRequest(requestId, user?.Username);

        return ToActionResult(response);
    }
   
}