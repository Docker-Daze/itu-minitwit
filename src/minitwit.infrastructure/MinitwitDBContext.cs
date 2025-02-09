using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MinitwitDbContext : IdentityDbContext<User>
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Follower> Followers { get; set; }
    
    public MinitwitDbContext(DbContextOptions<MinitwitDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.Tweets)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId);
        
        modelBuilder.Entity<Message>()
            .HasIndex(m => m.MessageId)
            .IsUnique();

        modelBuilder.Entity<Follower>()
            .HasNoKey();
    }
}