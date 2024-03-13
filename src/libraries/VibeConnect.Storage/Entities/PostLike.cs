namespace VibeConnect.Storage.Entities;

public class PostLike
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string PostId { get; set; } 
    
    /// <summary>
    /// Nullable foreign key to reference the liked comment
    /// </summary>
    public string? CommentId { get; set; } 
    public string UserId { get; set; } 
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    //Navigation Properties
    
    /// <summary>
    /// Navigation property for the liked post
    /// </summary>
    public virtual Post Post { get; set; }
    
    /// <summary>
    /// Navigation property for the liked comment
    /// </summary>
    public virtual Comment Comment { get; set; }
    
    /// <summary>
    /// Navigation property for the user who made the like
    /// </summary>
    public virtual User User { get; set; } 
}