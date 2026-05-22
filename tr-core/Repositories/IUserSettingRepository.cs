using System.Threading.Tasks;
using tr_core.Entities;

namespace tr_core.Repositories
{
    public interface IUserSettingRepository : IRepository<UserSetting>
    {
        Task<UserSetting?> GetByUserIdAsync(string userId);
    }
}
