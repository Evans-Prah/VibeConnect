using VibeConnect.Storage.Entities;

namespace VibeConnect.Post.Module.DTOs.Post;

public class PostResponseDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Content { get; set; }
    public ICollection<MediaContent> MediaContents { get; set; }
    public string Location { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}