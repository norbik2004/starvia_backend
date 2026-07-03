using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Consts;
using Core.Domain.Enums;

namespace Core.Application.DTO.PostPublication.Request
{
    public class PostPublicationParamsRequest : PaginatedListQueryParams
    {
        public int? PostId { get; set; }
        public int? UserPlatformId { get; set; }
        public PostPublicationStatus? Status { get; set; }
        public DateTime? PublishedBefore { get; set; }
        public DateTime? PublishedAfter { get; set; }
        public PlatformType? PublishedOn { get; set; }
        public PostPublishSortBy? SortBy { get; set; }
    }

    public enum PostPublishSortBy
    {
        CreatedAt,
        Platform,
    }
}
