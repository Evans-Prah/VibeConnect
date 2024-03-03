using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VibeConnect.Storage.Entities;

namespace VibeConnect.Storage.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(u => u.Username).IsUnique();
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(u => u.Email).IsUnique();
        
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);
        
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(250);
        
        builder.Property(u => u.Bio)
            .HasMaxLength(1000);

        builder.Property(u => u.ProfilePictureUrl)
            .HasMaxLength(500);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasColumnType("bytea")
            .HasConversion<byte[]>();

        builder.Property(u => u.Salt)
            .IsRequired()
            .HasColumnType("bytea")
            .HasConversion<byte[]>();
        
        builder.Property(u => u.PasswordResetToken)
            .HasMaxLength(255);

        builder.Property(u => u.AccountStatus)
            .IsRequired()
            .HasDefaultValue(AccountStatus.Active)
            .HasMaxLength(30)
            .HasConversion<string>();
        
        builder.Property(u => u.PrivacyLevel)
            .IsRequired()
            .HasDefaultValue(PrivacyLevel.Public)
            .HasMaxLength(30)
            .HasConversion<string>();
        
        builder.OwnsMany(u => u.ExternalLinks, el =>
        {
            el.ToJson();
            el.Property(l => l.Name).HasMaxLength(255);
            el.Property(l => l.Url).HasMaxLength(500);
        }); 
        
        builder.OwnsMany(u => u.LanguagePreferences, l =>
        {
            l.ToJson();
            l.Property(x => x.Language).HasMaxLength(150);
        });

        builder.OwnsOne(u => u.Location, l =>
        {
            l.ToJson();
            l.Property(lo => lo.City).HasMaxLength(255);
            l.Property(lo => lo.Country).HasMaxLength(255);
        });

        builder.HasMany(u => u.Posts)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade); 
        
        builder.HasMany(u => u.PostLikes)
            .WithOne(pl => pl.User)
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Comments)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}