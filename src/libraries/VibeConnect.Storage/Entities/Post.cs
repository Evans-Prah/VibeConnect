using System.ComponentModel.DataAnnotations;

namespace VibeConnect.Storage.Entities;

public class Post
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public required string UserId { get; set; }
    public required string Content { get; set; }

    public ICollection<MediaContent> MediaContents { get; set; } = new List<MediaContent>();
    public required string Location { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// Navigation property for the user who created the post
    /// </summary>
    public virtual User User { get; set; } 
    
    /// <summary>
    /// // Navigation property for post likes
    /// </summary>
    public virtual ICollection<PostLike> PostLikes { get; set; } 
    
    /// <summary>
    /// Navigation property for comments on the post
    /// </summary>
    public virtual ICollection<Comment> Comments { get; set; } 
}

public class MediaContent
{
    [RegularExpression("^(video|image|gif)$", ErrorMessage = "Type must be 'video', 'image', or 'gif'.")]
    public string? Type { get; set; }

    [Url(ErrorMessage = "Invalid URL format.")]
    public string? Url { get; set; }
}