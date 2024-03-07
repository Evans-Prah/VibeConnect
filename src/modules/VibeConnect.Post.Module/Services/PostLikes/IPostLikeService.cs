using VibeConnect.Post.Module.DTOs.Post;
using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.PostLikes;

public interface IPostLikeService
{
    Task<ApiResponse<int?>> HandlePostLike(string postId, string? username, bool isLike = false);
    Task<ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>> GetUsersWhoLikedPost(string postId, BaseFilter baseFilter);
}