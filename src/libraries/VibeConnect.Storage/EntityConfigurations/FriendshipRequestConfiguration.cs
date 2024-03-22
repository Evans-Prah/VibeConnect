using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeConnect.Storage.Entities;

namespace VibeConnect.Storage.EntityConfigurations;

public class FriendshipRequestConfiguration : IEntityTypeConfiguration<FriendshipRequest>
{
    public void Configure(EntityTypeBuilder<FriendshipRequest> builder)
    {
        builder.HasKey(fr => fr.Id);

        builder.HasOne(fr => fr.Sender)
            .WithMany(u => u.SentFriendshipRequests)
            .HasForeignKey(fr => fr.SenderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fr => fr.Receiver)
            .WithMany(u => u.ReceivedFriendshipRequests)
            .HasForeignKey(fr => fr.ReceiverId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}