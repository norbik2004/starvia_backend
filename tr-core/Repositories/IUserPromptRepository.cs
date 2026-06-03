using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Entities;

namespace tr_core.Repositories
{
    public interface IUserPromptRepository : IRepository<UserPrompt>
    {
        IQueryable<UserPrompt> GetUserPromptsAsQuery();
        IQueryable<UserPrompt> GetUserPromptsAsQueryPerUser(string userId);
    }
}
