using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeConnect.Storage.Entities;

namespace VibeConnect.Storage.EntityConfigurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.PostId)
            .IsRequired();
        
        builder.HasIndex(c => c.PostId);

        builder.Property(c => c.UserId)
            .IsRequired();
        
        builder.HasIndex(c => c.UserId);

        builder.Property(c => c.Content)
            .IsRequired();
        

        builder.HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.PostLikes)
            .WithOne(pl => pl.Comment)
            .HasForeignKey(pl => pl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}