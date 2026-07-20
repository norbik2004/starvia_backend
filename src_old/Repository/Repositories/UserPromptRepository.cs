using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Infrastructure.Repositories;
using Core.Domain.Enums;

namespace Repository.Repositories
{
    public class UserPromptRepository(TrDbContext dbContext) : IUserPromptRepository
    {
        public async Task AddAsync(UserPrompt entity)
        {
            await dbContext.AddAsync(entity);
        }

        public IQueryable<UserPrompt> GetAllAsQueryAsync()
        {
            return dbContext.UserPrompts.AsNoTracking();
        }

        public IQueryable<UserPrompt> GetAllAsQueryPerUserIdAsync(string userId)
        {
            return dbContext.UserPrompts.Where(up => up.UserId == userId).AsNoTracking();
        }

        public Task<List<UserPrompt>> GetAllAsync()
        {
            return dbContext.UserPrompts.ToListAsync();
        }

        public async Task<List<UserPrompt>> GetAllPerPostIdAndUserIdConversationVise(int postId, string userId)
        {
            return await dbContext.UserPrompts.Where(up => 
                (up.PostId == postId && up.UserId == userId)
                && up.ConversationType == GeminiConversationType.AskGemini)
                .ToListAsync();
        }

        public async Task<UserPrompt?> GetByIdAsync(string id)
        {
            return await dbContext.UserPrompts.FirstOrDefaultAsync(up => up.Id == Int32.Parse(id));
        }

        public void Remove(UserPrompt entity)
        {
            dbContext.UserPrompts.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(UserPrompt entity)
        {
            dbContext.UserPrompts.Update(entity);
        }
    }
}
