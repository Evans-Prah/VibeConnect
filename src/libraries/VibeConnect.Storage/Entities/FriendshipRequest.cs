namespace VibeConnect.Storage.Entities;

public class FriendshipRequest
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Navigation property for the user making the friendship request
    /// </summary>
    public virtual User Sender { get; set; }
    
    /// <summary>
    /// Navigation property for the user to whom the friendship request is been made to
    /// </summary>
    public virtual User Receiver { get; set; }
}