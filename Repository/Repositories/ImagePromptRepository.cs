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
    public class ImagePromptRepository(TrDbContext dbContext) : IImagePromptRepository
    {
        public async Task AddAsync(ImagePrompt entity)
        {
            await dbContext.ImagePrompts.AddAsync(entity);
        }

        public IQueryable<ImagePrompt> GetAllAsQueryAsync()
        {
            throw new NotImplementedException();
        }

        public IQueryable<ImagePrompt> GetAllAsQueryPerUserIdAsync(string userId)
        {
            return dbContext.ImagePrompts
                .Include(x => x.Conversation)
                .Where(x => x.Conversation.UserId == userId).AsNoTracking();
        }

        public async Task<List<ImagePrompt>> GetAllAsync()
        {
            return await dbContext.ImagePrompts.ToListAsync();
        }

        public async Task<ImagePrompt?> GetByIdAsync(string id)
        {
           return await dbContext.ImagePrompts
                .Include(x => x.ImagePromptFile)
                .FirstOrDefaultAsync(x => x.Id.ToString() == id);
        }

        public void Remove(ImagePrompt entity)
        {
            dbContext.ImagePrompts.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(ImagePrompt entity)
        {
            dbContext.Update(entity);
        }
    }
}
