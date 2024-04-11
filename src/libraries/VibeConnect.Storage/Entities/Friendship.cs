namespace VibeConnect.Storage.Entities;

public class Friendship
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public required string FollowerId { get; set; } 
    public required string FollowingId { get; set; }
    public DateTimeOffset FollowedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsMutual { get; set; }
    /// <summary>
    /// Navigation property for the user/users who have followed the current logged-in user
    /// </summary>
    public virtual User Follower { get; set; }
    
    /// <summary>
    /// Navigation property for the user/users who have been followed the current logged-in user
    /// </summary>
    public virtual User Following { get; set; }
}