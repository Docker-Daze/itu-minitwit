using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace minitwit.infrastructure;

public class MinitwitDbContextFactory : IDesignTimeDbContextFactory<MinitwitDbContext>
{
    public MinitwitDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MinitwitDbContext>();
            
        optionsBuilder.UseSqlite("Data Source=minitwit.db");

        return new MinitwitDbContext(optionsBuilder.Options);
    }
}