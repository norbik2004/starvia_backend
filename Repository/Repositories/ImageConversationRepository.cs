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
    public class ImageConversationRepository(TrDbContext trDbContext) : IImageConversationRepository
    {
        public async Task AddAsync(ImageConversation entity)
        {
            await trDbContext.ImageConversations.AddAsync(entity);
        }

        public IQueryable<ImageConversation> GetAllAsQueryAsync()
        {
            throw new NotImplementedException();
        }

        public IQueryable<ImageConversation> GetAllAsQueryPerUserIdAsync(string userId)
        {
            return trDbContext.ImageConversations.Where(x => x.UserId == userId).AsNoTracking();
        }

        public Task<List<ImageConversation>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ImageConversation?> GetByIdAsync(string id)
        {
            return await trDbContext.ImageConversations
                .Include(x => x.ImagePrompts)
                .ThenInclude(x => x.ImagePromptFile)
                .FirstOrDefaultAsync(x => x.Id.ToString() == id);
        }

        public void Remove(ImageConversation entity)
        {
            trDbContext.ImageConversations.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await trDbContext.SaveChangesAsync();
        }

        public void Update(ImageConversation entity)
        {
            trDbContext.Update(entity);
        }
    }
}
