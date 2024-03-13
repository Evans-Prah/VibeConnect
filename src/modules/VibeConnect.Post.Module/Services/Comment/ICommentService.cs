using VibeConnect.Post.Module.DTOs.Comment;
using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.Comment;

public interface ICommentService
{
    Task<ApiResponse<CommentResponseDto>> AddComment(string? username, CommentRequestDto commentRequestDto);
    Task<ApiResponse<List<CommentNode>>> GetPostComments(string postId);
    Task<ApiResponse<bool>> DeleteComment(string commentId, string? username);
    Task<ApiResponse<CommentNode>> GetCommentWithReplies(string commentId);
}