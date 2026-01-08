using BlogPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<PostEntity>
{
    public void Configure(EntityTypeBuilder<PostEntity> builder)
    {
        builder.ToTable("articles");  // FIX: Correct table name
        builder.HasKey(p => p.PostId);  // FIX: Explicit PK
        
        builder.Property(p => p.PostId)
               .HasColumnName("post_id");  // FIX: snake_case
        
        builder.Property(p => p.ParentId)
               .HasColumnName("blog_id");  // FIX: FK naming
        
        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(50)
               .HasColumnName("name");
        
        builder.Property(p => p.Content)
               .IsRequired()
               .HasMaxLength(1000)
               .HasColumnName("content");
        
        builder.Property(p => p.Created)
               .HasColumnName("created");
        
        builder.Property(p => p.Updated)
               .HasColumnName("updated");
    }
}

