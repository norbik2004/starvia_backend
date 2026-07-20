using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Infrastructure.Repositories
{
    public interface IUserSettingRepository : IRepository<UserSetting>
    {
        Task<UserSetting?> GetByUserIdAsync(string userId);
    }
}
