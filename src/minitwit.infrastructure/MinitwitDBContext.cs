using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MinitwitDbContext : DbContext
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Follower> Followers { get; set; }
    
    public MinitwitDbContext(DbContextOptions<MinitwitDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Add indexes here
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username);
        modelBuilder.Entity<Message>()
            .HasIndex(m => m.MessageId)
            .IsUnique();
    }
}