namespace VibeConnect.Post.Module.DTOs.Comment;

public class CommentNode
{
    public string? CommentId { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Content { get; set; }
    public List<CommentNode> Replies { get; set; } = [];
    public int LikeCount { get; set; }

}
