using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Chirp.Infrastructure
{
    public class ChirpDbContextFactory : IDesignTimeDbContextFactory<ChirpDBContext>
    {
        public ChirpDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ChirpDBContext>();
            
            optionsBuilder.UseSqlite("Data Source=Cheeps.db");

            return new ChirpDBContext(optionsBuilder.Options);
        }
    }
}