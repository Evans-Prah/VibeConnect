using VibeConnect.Post.Module.DTOs.Comment;
using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.Comment;

public interface ICommentService
{
    Task<ApiResponse<CommentResponseDto>> AddComment(string? username, CommentRequestDto commentRequestDto);
    Task<ApiResponse<ApiPagedResult<CommentNode>>>GetPostComments(string postId, BaseFilter baseFilter);
    Task<ApiResponse<bool>> DeleteComment(string commentId, string? username);
    Task<ApiResponse<CommentNode>> GetCommentWithReplies(string commentId);
}