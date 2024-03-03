namespace VibeConnect.Storage.Entities;

public class Comment
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    /// <summary>
    /// Nullable foreign key to reference the parent comment
    /// </summary>
    public string? ParentCommentId { get; set; }
    public string PostId { get; set; }
    public string UserId { get; set; }
    public string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    //Navigation Properties

    /// <summary>
    /// Navigation property to represent nested (child) comments
    /// </summary>
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    
    /// <summary>
    /// Navigation property to represent the parent comment
    /// </summary>
    public Comment ParentComment { get; set; } 
    
    /// <summary>
    /// Navigation property for the post on which the comment is made
    /// </summary>
    public Post Post { get; set; } 
    
    /// <summary>
    /// Navigation property for the user who made the comment
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Navigation property for likes on this comment
    /// </summary>
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
}
