using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using minitwit.core;

namespace minitwit.infrastructure;

public class MinitwitDbContext : IdentityDbContext<User>
{
    public DbSet<Message> Messages { get; set; }
    public override DbSet<User> Users { get; set; }
    public DbSet<Follower> Followers { get; set; }

    public MinitwitDbContext(DbContextOptions<MinitwitDbContext> options) : base(options)
    {
        Messages = Set<Message>();
        Users = Set<User>();
        Followers = Set<Follower>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName != null)
            {
                entity.SetTableName(tableName.ToLower());
            }

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (columnName != null)
                {
                    property.SetColumnName(columnName.ToLower());
                }
            }
        }

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.EmailConfirmed)
                .HasColumnType("boolean");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.Property(m => m.PubDate)
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Follower>()
            .HasKey(f => new { f.WhoId, f.WhomId });
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();
        modelBuilder.Entity<Message>()
            .HasIndex(m => m.MessageId)
            .IsUnique();
        modelBuilder.Entity<Message>()
            .HasIndex(m => m.PubDate);

    }
}