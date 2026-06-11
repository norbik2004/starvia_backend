using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Repositories
{
    public interface IRepository<T>
    {
        Task<T?> GetByIdAsync(string id);
        Task<List<T>> GetAllAsync();
        IQueryable<T> GetAllAsQueryAsync();
        IQueryable<T> GetAllAsQueryPerUserIdAsync(string userId);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task SaveChangesAsync();
    }
}
