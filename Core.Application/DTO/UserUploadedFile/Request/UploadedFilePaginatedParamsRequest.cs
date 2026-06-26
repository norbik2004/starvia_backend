using Core.Domain.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.UserUploadedFile.Request
{
    public class UploadedFilePaginatedParamsRequest : PaginatedListQueryParams
    {
        public DateTime? CreatedBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public UserUploadFileSortBy? SortBy { get; set; }
    }

    public enum UserUploadFileSortBy
    {
        Id,
        CreatedAt,
    }
}
