using BlogPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Configurations;

public class CompromisedRefreshTokenConfiguration : IEntityTypeConfiguration<CompromisedRefreshToken>
{
    public void Configure(EntityTypeBuilder<CompromisedRefreshToken> builder)
    {
        builder.HasKey(t => t.Id);
        
        // Index for fast lookup when checking if token is compromised
        builder.HasIndex(t => t.TokenHash).IsUnique();
        
        // Index for efficient cleanup of expired tokens
        builder.HasIndex(t => t.ExpiresAt);
        
        builder.Property(t => t.TokenHash)
            .HasMaxLength(512)
            .IsRequired();
            
        builder.Property(t => t.Reason)
            .HasMaxLength(100);
    }
}

