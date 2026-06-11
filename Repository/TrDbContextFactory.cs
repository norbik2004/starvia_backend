using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
