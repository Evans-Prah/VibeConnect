using System.ComponentModel.DataAnnotations;

namespace VibeConnect.Friendship.Module.DTOs;

public class FriendRequestDto
{
    [Required]
    public required string ReceiverUsername { get; set; }
}