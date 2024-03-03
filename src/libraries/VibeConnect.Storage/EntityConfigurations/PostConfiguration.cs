using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VibeConnect.Storage.Entities;

namespace VibeConnect.Storage.EntityConfigurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId).IsRequired();

        builder.HasIndex(u => u.UserId);

        builder.Property(p => p.Content).IsRequired();
        
        builder.Property(p => p.Location)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.OwnsMany(p => p.MediaContents, mc =>
        {
            mc.ToJson();
            mc.Property(m => m.Type).HasMaxLength(50);
            mc.Property(m => m.Url).HasMaxLength(500);
        });

        builder.HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PostLikes)
            .WithOne(pl => pl.Post)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
    }
}