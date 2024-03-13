using System.ComponentModel.DataAnnotations;

namespace VibeConnect.Post.Module.DTOs.Comment;

public class CommentRequestDto
{
    [Required]
    public required string PostId { get; set; }
    [Required]
    public required string Content { get; set; }
    public string? ParentCommentId { get; set; }

}