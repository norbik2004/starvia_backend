using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Entities;
using tr_core.Enums;
using tr_core.Repositories;

namespace tr_repository.Repositories
{
    public class PlatformRepository(TrDbContext dbContext) : IPlatformRepository
    {
        public async Task AddAsync(Platform entity)
        {
            await dbContext.Platforms.AddAsync(entity);
        }

        public async Task<List<Platform>> GetAllAsync()
        {
            return await dbContext.Platforms.ToListAsync();
        }

        public async Task<Platform?> GetByIdAsync(string id)
        {
            return await dbContext.Platforms.FirstOrDefaultAsync(p => p.Id.ToString() == id);
        }

        public async Task<Platform?> GetPlatformByTypeAsync(PlatformType platformType)
        {
            return await dbContext.Platforms.FirstOrDefaultAsync(p => p.Type == platformType);
        }

        public void Remove(Platform entity)
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public void Update(Platform entity)
        {
            throw new NotImplementedException();
        }
    }
}
