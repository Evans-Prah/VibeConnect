using VibeConnect.Friendship.Module.DTOs;
using VibeConnect.Shared.Models;

namespace VibeConnect.Friendship.Module.Services;

public interface IFriendRequestService
{
    Task<ApiResponse<object>> SendFriendRequest(string? senderUsername,FriendRequestDto friendRequestDto);
    Task<ApiResponse<ApiPagedResult<FriendRequestResponseDto>>> GetFriendRequests(string? username, BaseFilter baseFilter, bool sentRequests);
    Task<ApiResponse<bool>> ApproveFriendRequest(string requestId, string? username);
    Task<ApiResponse<bool>> RejectFriendRequest(string requestId, string? username);
}