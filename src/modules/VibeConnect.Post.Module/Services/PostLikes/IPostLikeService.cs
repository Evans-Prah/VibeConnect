using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.PostLikes;

public interface IPostLikeService
{
    Task<ApiResponse<int?>> HandlePostLike(string postId, string? username, bool isLike = false);
}