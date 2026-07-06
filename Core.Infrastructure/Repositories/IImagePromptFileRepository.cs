using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Repositories
{
    public interface IImagePromptFileRepository : IRepository<ImagePromptFile>
    {
        public ImagePromptFile? GetByIdAndUserId(Guid fileId, string userId);
    }
}
