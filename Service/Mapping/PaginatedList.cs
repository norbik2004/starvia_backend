using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using AutoMapper;
using Service.Exceptions;

namespace Service.Mapping
{
    public class PaginatedList<T>
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; } = [];

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            ValidateParams(pageIndex, pageSize);
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;

            if (PageIndex > TotalPages && TotalPages != 0)
                throw new BadRequestException("PageIndex cannot be greater than TotalPages");
        }

        public PaginatedList()
        {
        }

        public static async Task<PaginatedList<T>> CreateAsync<TSource>(IQueryable<TSource> source, IMapper mapper,
            int pageIndex, int pageSize)
        {
            if (pageIndex <= 0)
                throw new BadRequestException("Invalid PageIndex data");
            if (pageSize is <= 0 or > 1000)
                throw new BadRequestException("Invalid PageSize data");

            if (source.Provider is IAsyncQueryProvider)
            {
                var count = await source.CountAsync();
                var items = mapper.Map<List<T>>(await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync());
                return new PaginatedList<T>(items, count, pageIndex, pageSize);
            }

            // For tests and non-async sources
            var syncCount = source.Count();
            var syncItems = mapper.Map<List<T>>(source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
            return await Task.FromResult(new PaginatedList<T>(syncItems, syncCount, pageIndex, pageSize));
        }

        private void ValidateParams(int pageIndex, int pageSize)
        {
            if (pageIndex <= 0 || pageIndex > 1000)
                throw new BadRequestException("Invalid PageIndex data");

            if (pageSize <= 0 || pageSize > 1000)
                throw new BadRequestException("Invalid PageSize data");
        }
    }
}
