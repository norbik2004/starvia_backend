using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Repositories
{
    public interface IUserUploadedFileRepository : IRepository<UserUploadedFile>
    {
        public Task<UserUploadedFile?> GetByIdAndUserIdAsync(Guid fileId, string userId);
    }
}
