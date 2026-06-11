using System.Threading.Tasks;
using Core.Entities;

namespace Core.Repositories
{
    public interface IUserSettingRepository : IRepository<UserSetting>
    {
        Task<UserSetting?> GetByUserIdAsync(string userId);
    }
}
