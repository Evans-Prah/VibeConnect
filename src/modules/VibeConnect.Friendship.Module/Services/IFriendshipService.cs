using VibeConnect.Friendship.Module.DTOs;
using VibeConnect.Shared.Models;

namespace VibeConnect.Friendship.Module.Services;

public interface IFriendshipService
{
    Task<ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>> GetUserFollowers(string username, BaseFilter baseFilter);
    Task<ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>> GetUserFollowing(string username, BaseFilter baseFilter);
    Task<ApiResponse<bool>> UnfollowUserTransaction(string username, string followingUsername);
    Task<ApiResponse<bool>> UnfollowUser(string username, string followingUsername);
}