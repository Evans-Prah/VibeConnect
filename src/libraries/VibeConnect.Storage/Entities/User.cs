namespace VibeConnect.Storage.Entities;

public class User
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string Username { get; set; } 
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string FullName { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] Salt { get; set; }
    
    public string? PasswordResetToken { get; set; }
    
    public DateTimeOffset? PasswordResetTokenExpiry { get; set; }
   
    public DateTimeOffset CreatedAt{ get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset? LastLoginDate { get; set; }
    public string AccountStatus { get; set; }
    public string PrivacyLevel { get; set; }

    public List<LanguagePreference>? LanguagePreferences { get; set; } = [];
    public int TotalPosts { get; set; }
    public int TotalFollowers { get; set; }
    public int TotalFollowing { get; set; }
    public DateTimeOffset LastActivityDate { get; set; } = DateTimeOffset.UtcNow;
    public bool IsVerified { get; set; }
    public bool IsSuspended { get; set; }
    public List<ExternalLink>? ExternalLinks { get; set; } = [];
    
    //public string NotificationPreferences { get; set; }
    //public string ThemePreferences { get; set; }

    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenAddedAt { get; set; }
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }

    public Location? Location { get; set; } = new();
    
    
    // Navigation Properties
    
    /// <summary>
    /// Navigation property for posts created by the user
    /// </summary>
    public ICollection<Post> Posts { get; } = new List<Post>();

    /// <summary>
    /// Navigation property for user's post likes
    /// </summary>
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    /// <summary>
    /// Navigation property for user's comments
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

}

public class ExternalLink
{
    public string? Name { get; set; }
    public string? Url { get; set; }
}

public class LanguagePreference
{
    public string? Language { get; set; }
}

public enum AccountStatus
{
    Active = 0,
    Suspended = 1,
    Deactivated = 2
}

public enum PrivacyLevel
{
    Public = 0,
    Private = 1
}

public class Location
{
    public string? City { get; set; }
    public string? Country { get; set; }
}