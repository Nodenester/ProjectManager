using Microsoft.EntityFrameworkCore;
using ProjectManager.Models;

namespace ProjectManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<WhitelistedUser> WhitelistedUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WhitelistedUser>(entity =>
        {
            entity.HasIndex(e => e.GitHubUsername).IsUnique();

            entity.HasData(new WhitelistedUser
            {
                Id = 1,
                GitHubUsername = "NodeNestor",
                AddedAt = DateTime.UtcNow,
                IsActive = true
            });
        });
    }
}