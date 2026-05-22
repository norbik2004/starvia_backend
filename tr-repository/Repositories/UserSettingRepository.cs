using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using tr_core.Entities;
using tr_core.Repositories;

namespace tr_repository.Repositories
{
    public class UserSettingRepository(TrDbContext dbContext) : IUserSettingRepository
    {
        public async Task AddAsync(UserSetting entity)
        {
            await dbContext.UserSettings.AddAsync(entity);
        }

        public async Task<List<UserSetting>> GetAllAsync()
        {
            return await dbContext.UserSettings.ToListAsync();
        }

        public async Task<UserSetting?> GetByIdAsync(string id)
        {
            var setting = await dbContext.UserSettings.FirstOrDefaultAsync(s => s.UserId == id);
            return setting;
        }

        public async Task<UserSetting?> GetByUserIdAsync(string userId)
        {
            return await dbContext.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public void Remove(UserSetting entity)
        {
            dbContext.UserSettings.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(UserSetting entity)
        {
            dbContext.UserSettings.Update(entity);
        }
    }
}
