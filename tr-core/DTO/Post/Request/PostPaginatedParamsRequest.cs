using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Consts;
using tr_core.Enums;

namespace tr_core.DTO.Post.Request
{
    public class PostPaginatedParamsRequest : PaginatedListQueryParams
    {
        public PostStatus? Status { get; set; }
        public bool? HasPublication { get; set; }
        public string? TitleContains { get; set; }
        public string? BodyContains { get; set; }
        public string? UserId { get; set; }
        public PlatformType? PublishedOn { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public PostSortBy? SortBy { get; set; }
    }

    public enum PostSortBy
    {
        Id,
        CreatedBy,
        Status,
        UpdatedBy,
    }
}
