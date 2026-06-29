using Core.Domain.Entities;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Repository.Repositories
{
    public class UserUploadedFileRepository(TrDbContext dbContext) : IUserUploadedFileRepository
    {
        public async Task AddAsync(UserUploadedFile entity)
        {
            await dbContext.AddAsync(entity);
        }

        public IQueryable<UserUploadedFile> GetAllAsQueryAsync()
        {
            throw new NotImplementedException();
        }

        public IQueryable<UserUploadedFile> GetAllAsQueryPerUserIdAsync(string userId)
        {
            return dbContext.UserUploadedFiles.Where(x => x.UserId == userId);
        }

        public async Task<List<UserUploadedFile>> GetAllAsync()
        {
            return await dbContext.UserUploadedFiles.ToListAsync();
        }

        public async Task<UserUploadedFile?> GetByIdAndUserIdAsync(Guid fileId, string userId)
        {
            return await dbContext.UserUploadedFiles.FirstOrDefaultAsync(x => x.Id == fileId && x.UserId == userId);
        }

        public async Task<UserUploadedFile?> GetByIdAsync(string id)
        {
            return await dbContext.UserUploadedFiles.FirstOrDefaultAsync(x => x.Id.ToString() == id);
        }

        public void Remove(UserUploadedFile entity)
        {
            dbContext.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(UserUploadedFile entity)
        {
            dbContext.Update(entity);
        }
    }
}
