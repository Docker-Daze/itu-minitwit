using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MinitwitDbContext : IdentityDbContext<User>
{
    public DbSet<Message> Messages { get; set; }
    public override DbSet<User> Users { get; set; }
    public DbSet<Follower> Followers { get; set; }
    
    public MinitwitDbContext(DbContextOptions<MinitwitDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Follower>()
            .HasKey(f => new { f.WhoId, f.WhomId });
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();
        modelBuilder.Entity<Message>()
            .HasIndex(m => m.MessageId)
            .IsUnique();
        
    }
}