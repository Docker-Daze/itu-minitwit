using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MinitwitDbContext : DbContext
{
    public MinitwitDbContext(DbContextOptions<MinitwitDbContext> options) : base(options) { }
    
    public DbSet<Message> Messages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Follower> Followers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Follower>()
            .HasKey(f => new { f.WhoId, f.WhomId });
        // Add indexes here
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username);
        modelBuilder.Entity<Message>()
            .HasIndex(m => m.MessageId)
            .IsUnique();
    }
}