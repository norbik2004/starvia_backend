using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Infrastructure.Repositories
{
    public interface IUserPromptRepository : IRepository<UserPrompt>
    {
        public Task<List<UserPrompt>> GetAllPerPostIdAndUserIdConversationVise(int postId, string userId);
    }
}
