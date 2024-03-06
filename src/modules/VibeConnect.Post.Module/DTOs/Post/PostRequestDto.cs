using System.ComponentModel.DataAnnotations;
using VibeConnect.Storage.Entities;

namespace VibeConnect.Post.Module.DTOs.Post;

public class PostRequestDto
{
    [Required]
    public required string Content { get; set; }
    
    public ICollection<MediaContent> MediaContents { get; set; }
    
    [Required]
    public required string Location { get; set; }
}

public class MediaContentDto
{
    [RegularExpression("^(video|image|gif)$", ErrorMessage = "Type must be 'video', 'image', or 'gif'.")]
    public string? Type { get; set; }

    [Url(ErrorMessage = "Invalid URL format.")]
    public string? Url { get; set; }
}