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
    public class UserRepository(TrDbContext dbContext) : IUserRepository
    {
        public Task AddAsync(User entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<User> GetAllAsQueryAsync()
        {
            return dbContext.Users.AsNoTracking();
        }

        public IQueryable<User> GetAllAsQueryPerUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await dbContext.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("User id cannot be empty", nameof(id));

            var user = await dbContext.Users
                .Include(u => u.Posts)
                .ThenInclude(p => p.PostPublications)
                .Include(u => u.UserSettings)
                .Include(u => u.UserPrompts)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public void Remove(User entity)
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
