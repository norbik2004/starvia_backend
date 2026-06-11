using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Repository
{
    public class TrDbContextFactory : IDesignTimeDbContextFactory<TrDbContext>
    {
        public TrDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TrDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5435;Database=postgres;Username=postgres;Password=postgres");

            return new TrDbContext(optionsBuilder.Options);
        }
    }
}
