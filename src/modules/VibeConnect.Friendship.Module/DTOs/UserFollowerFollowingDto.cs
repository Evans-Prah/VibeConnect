namespace VibeConnect.Friendship.Module.DTOs;

public class UserFollowerFollowingDto
{
    public string Username { get; set; }
    public string FullName { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsMutual { get; set; }
}