namespace VibeConnect.Post.Module.DTOs.Comment;

public class CommentResponseDto
{
    public string? Id { get; set; }
    public string? ParentCommentId { get; set; }
    public string? PostId { get; set; }
    public string? UserId { get; set; }
    public string? Content { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}