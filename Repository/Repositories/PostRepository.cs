using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Infrastructure.Repositories;

namespace Repository.Repositories
{
    public class PostRepository(TrDbContext dbContext) : IPostRepository
    {
        public async Task AddAsync(Post entity)
        {
            await dbContext.Posts.AddAsync(entity);
        }

        public IQueryable<Post> GetAllAsQueryAsync()
        {
            return dbContext.Posts.AsNoTracking();
        }

        public IQueryable<Post> GetAllAsQueryPerUserIdAsync(string userId)
        {
            return dbContext.Posts.Where(p => p.UserId == userId).AsNoTracking();
        }

        public async Task<List<Post>> GetAllAsync()
        {
            return await dbContext.Posts.ToListAsync();
        }

        public async Task<Post?> GetByIdAsync(string id)
        {
            int identifier = Int32.Parse(id);

            var post = await dbContext.Posts
                .Include(up => up.User)
                .ThenInclude(up => up.UserPrompts)
                .FirstOrDefaultAsync(p => p.Id == identifier);

            return post;
        }

        public void Remove(Post entity)
        {
            dbContext.Posts.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(Post entity)
        {
            dbContext.Posts.Update(entity);
        }
    }
}
