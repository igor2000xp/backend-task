using BlogPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogPlatform.Infrastructure.Configurations;

public class BlogConfiguration : IEntityTypeConfiguration<BlogEntity>
{
    public void Configure(EntityTypeBuilder<BlogEntity> builder)
    {
        builder.ToTable("blogs");
        builder.HasKey(b => b.BlogId);  // FIX: Explicit PK
        
        builder.Property(b => b.BlogId)
               .HasColumnName("blog_id");  // FIX: snake_case
        
        builder.Property(b => b.Name)
               .IsRequired()
               .HasMaxLength(50)
               .HasColumnName("name");
        
        // FIX: Custom value conversion for IsActive
        builder.Property(b => b.IsActive)
               .HasColumnName("is_active")
               .HasConversion(
                   v => v ? "Blog is active" : "Blog is not active",
                   v => v == "Blog is active"
               );
        
        // FIX: Explicit relationship mapping to ParentId
        builder.HasMany(b => b.Articles)
               .WithOne(p => p.Blog)
               .HasForeignKey(p => p.ParentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

