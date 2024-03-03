using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeConnect.Storage.Entities;

namespace VibeConnect.Storage.EntityConfigurations;

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.PostId)
            .IsRequired();

        builder.HasIndex(pl => pl.PostId);

        builder.Property(pl => pl.UserId)
            .IsRequired();
        
        builder.HasIndex(pl => pl.UserId);

        builder.HasOne(pl => pl.Post)
            .WithMany(p => p.PostLikes)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pl => pl.User)
            .WithMany(u => u.PostLikes)
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pl => pl.Comment)
            .WithMany(c => c.PostLikes)
            .HasForeignKey(pl => pl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}