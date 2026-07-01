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
    public class PostAttachmentRepository(TrDbContext dbContext) : IPostAttachmentRepository
    {
        public async Task AddAsync(PostAttachment entity)
        {
            await dbContext.AddAsync(entity);
        }

        public IQueryable<PostAttachment> GetAllAsQueryAsync()
        {
            return dbContext.PostAttachments.AsNoTracking();
        }

        public IQueryable<PostAttachment> GetAllAsQueryPerUserIdAsync(string userId)
        {
            return dbContext.PostAttachments
                .Include(c => c.Post)
                .Where(up => up.Post.UserId == userId).AsNoTracking();
        }

        public async Task<List<PostAttachment>> GetAllAsync()
        {
            return await dbContext.PostAttachments.ToListAsync();
        }

        public async Task<List<PostAttachment>> GetAllPerPostAndUserId(int postId, string userId)
        {
            return await dbContext.PostAttachments
                .Include(c => c.Post)
                .Where(up => up.Post.UserId == userId && up.PostId == postId)
                .ToListAsync();
        }

        public async Task<PostAttachment?> GetByIdAsync(string id)
        {
            return await dbContext.PostAttachments.FirstOrDefaultAsync(pa => pa.Id.ToString() == id);
        }

        public void Remove(PostAttachment entity)
        {
            dbContext.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(PostAttachment entity)
        {
            dbContext.Update(entity);
        }
    }
}
