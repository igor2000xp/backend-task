using System.ComponentModel.DataAnnotations;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Data;

public class BlogsContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<BlogEntity> Blogs { get; set; } = null!;
    public DbSet<PostEntity> Posts { get; set; } = null!;
    public DbSet<CompromisedRefreshToken> CompromisedRefreshTokens { get; set; } = null!;
    
    public BlogsContext(DbContextOptions<BlogsContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CRITICAL: Call base Identity configuration first
        base.OnModelCreating(modelBuilder);
        
        // Apply existing configurations
        modelBuilder.ApplyConfiguration(new BlogConfiguration());
        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ApplyConfiguration(new CompromisedRefreshTokenConfiguration());
        
        // Configure Blog -> User relationship
        modelBuilder.Entity<BlogEntity>()
            .HasOne(b => b.User)
            .WithMany(u => u.Blogs)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure Post -> User relationship
        modelBuilder.Entity<PostEntity>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete conflicts with Blog cascade
    }
    
    // FIX: Validation pipeline enforcement
    public override int SaveChanges()
    {
        ValidateEntities();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateEntities();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void ValidateEntities()
    {
        var entities = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity);
        
        foreach (var entity in entities)
        {
            var validationContext = new ValidationContext(entity);
            Validator.ValidateObject(entity, validationContext, validateAllProperties: true);
        }
    }
}
