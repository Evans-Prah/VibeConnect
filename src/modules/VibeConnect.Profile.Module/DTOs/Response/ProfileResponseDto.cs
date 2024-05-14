using VibeConnect.Storage.Entities;

namespace VibeConnect.Profile.Module.DTOs.Response;

public class ProfileResponseDto
{
    public string? Username { get; set; } 
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
   
    public DateTimeOffset CreatedAt{ get; set; }
    
    public DateTimeOffset? LastLoginDate { get; set; }
    public string? AccountStatus { get; set; }
    public string? PrivacyLevel { get; set; }
   
    public List<LanguagePreference>? LanguagePreferences { get; set; }
    public int TotalPosts { get; set; }
    public int TotalFollowers { get; set; }
    public int TotalFollowing { get; set; }
    public DateTimeOffset LastActivityDate { get; set; }
    public bool IsVerified { get; set; }
    public List<ExternalLink>? ExternalLinks { get; set; }
    public Location? Location { get; set; }
}