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
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    
    /// <summary>
    /// Navigation property to represent the parent comment
    /// </summary>
    public virtual Comment ParentComment { get; set; } 
    
    /// <summary>
    /// Navigation property for the post on which the comment is made
    /// </summary>
    public virtual Post Post { get; set; } 
    
    /// <summary>
    /// Navigation property for the user who made the comment
    /// </summary>
    public virtual User User { get; set; }

    /// <summary>
    /// Navigation property for likes on this comment
    /// </summary>
    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
}
