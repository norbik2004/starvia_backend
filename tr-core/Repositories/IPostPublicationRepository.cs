using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Entities;

namespace tr_core.Repositories
{
    public interface IPostPublicationRepository : IRepository<PostPublication>
    {
        public Task<List<PostPublication>> GetPostPublicationsPerUser(string userId);
    }
}
