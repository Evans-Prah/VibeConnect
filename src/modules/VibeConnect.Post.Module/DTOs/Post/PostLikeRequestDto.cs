using System.ComponentModel.DataAnnotations;

namespace VibeConnect.Post.Module.DTOs.Post;

public class PostLikeRequestDto
{
    /// <summary>
    /// The ID of the post being liked/unliked
    /// </summary>
    [Required]
    public required string PostId { get; set; } 
    /// <summary>
    /// Indicates whether it's a like (true) or unlike (false)
    /// </summary>
    [Required]
    public bool IsLike { get; set; } 
}