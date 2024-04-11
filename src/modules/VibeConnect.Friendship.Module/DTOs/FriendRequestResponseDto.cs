namespace VibeConnect.Friendship.Module.DTOs;

public class FriendRequestResponseDto
{
    public string Id { get; set; }
    public FriendRequestUserDto User { get; set; }
}

public class FriendRequestUserDto
{
    public string Id { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
}