using VibeConnect.Post.Module.DTOs.Post;
using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.Comment;

public interface ICommentLikeService
{
    Task<ApiResponse<int>> HandleCommentLike(string commentId, string? username, bool isLike = false);
    Task<ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>> GetUsersWhoLikedComment(string commentId, BaseFilter baseFilter);
}