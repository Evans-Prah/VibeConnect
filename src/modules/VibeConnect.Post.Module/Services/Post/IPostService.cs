using VibeConnect.Post.Module.DTOs.Post;
using VibeConnect.Shared.Models;

namespace VibeConnect.Post.Module.Services.Post;

public interface IPostService
{
    Task<ApiResponse<PostResponseDto>> CreatePost(string? username, PostRequestDto postRequestDto);
    Task<ApiResponse<ApiPagedResult<PostResponseDto>>> GetUserPosts(BaseFilter baseFilter, string? username = null);
    Task<ApiResponse<PostResponseDto>> GetUserPost(string postId, string? username = null);
    Task<ApiResponse<PostResponseDto>> UpdatePost(string? username, string postId, PostRequestDto postRequestDto);
    Task<ApiResponse<bool>> DeletePost(string? username, string postId);
}