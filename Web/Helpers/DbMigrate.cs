using Microsoft.EntityFrameworkCore;
using Repository;

namespace Web.Helpers
{
    public static class DbMigrate
    {
        public async static Task MigrateDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TrDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}
