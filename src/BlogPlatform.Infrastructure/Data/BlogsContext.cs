using System.ComponentModel.DataAnnotations;
using BlogPlatform.Domain.Entities;
using BlogPlatform.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BlogPlatform.Infrastructure.Data;

public class BlogsContext : DbContext
{
    public DbSet<BlogEntity> Blogs { get; set; } = null!;
    public DbSet<PostEntity> Posts { get; set; } = null!;
    
    public BlogsContext(DbContextOptions<BlogsContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BlogConfiguration());
        modelBuilder.ApplyConfiguration(new PostConfiguration());
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

