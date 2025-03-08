using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace minitwit.infrastructure;

public class MinitwitDbContextFactory : IDesignTimeDbContextFactory<MinitwitDbContext>
{
    public MinitwitDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MinitwitDbContext>();
            
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<MinitwitDbContextFactory>()
            .Build();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseNpgsql(connectionString);
        
        return new MinitwitDbContext(optionsBuilder.Options);
    }
}