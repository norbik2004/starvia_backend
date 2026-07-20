using Core.Domain.Entities;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class ImagePromptFileRepository(TrDbContext dbContext) : IImagePromptFileRepository
    {
        public Task AddAsync(ImagePromptFile entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ImagePromptFile> GetAllAsQueryAsync()
        {
            throw new NotImplementedException();
        }

        public IQueryable<ImagePromptFile> GetAllAsQueryPerUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ImagePromptFile>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public ImagePromptFile? GetByIdAndUserId(Guid fileId, string userId)
        {
            return dbContext.ImagePromptFiles
                .Include(x => x.ImagePrompt)
                .ThenInclude(x => x.Conversation)
                .FirstOrDefault(x => x.Id == fileId && x.ImagePrompt.Conversation.UserId == userId);
        }

        public Task<ImagePromptFile?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public void Remove(ImagePromptFile entity)
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public void Update(ImagePromptFile entity)
        {
            throw new NotImplementedException();
        }
    }
}
