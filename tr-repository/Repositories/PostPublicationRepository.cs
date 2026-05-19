using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Entities;
using tr_core.Repositories;

namespace tr_repository.Repositories
{
    public class PostPublicationRepository(TrDbContext dbContext) : IPostPublicationRepository
    {
        public async Task AddAsync(PostPublication entity)
        {
            await dbContext.PostPublications.AddAsync(entity);
        }

        public async Task<List<PostPublication>> GetAllAsync()
        {
            return await dbContext.PostPublications.ToListAsync();
        }

        public async Task<PostPublication?> GetByIdAsync(string id)
        {
            return await dbContext.PostPublications.FirstOrDefaultAsync(pb => pb.Id.ToString() == id);
        }

        public async Task<List<PostPublication>> GetPostPublicationsPerUser(string userId)
        {
            return await dbContext.PostPublications.Where(pb => pb.UserPlatform.UserId == userId)
                .ToListAsync();
        }

        public void Remove(PostPublication entity)
        {
            dbContext.PostPublications.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(PostPublication entity)
        {
            dbContext.PostPublications.Update(entity);
        }
    }
}
