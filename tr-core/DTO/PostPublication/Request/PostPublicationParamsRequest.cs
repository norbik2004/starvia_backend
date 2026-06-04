using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Consts;
using tr_core.Enums;

namespace tr_core.DTO.PostPublication.Request
{
    public class PostPublicationParamsRequest : PaginatedListQueryParams
    {
        public int? PostId { get; set; }
        public int? UserPlatformId { get; set; }
        public PostPublicationStatus? Status { get; set; }
        public DateTime? PublishedBefore { get; set; }
        public DateTime? PublishedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public PlatformType? PublishedOn { get; set; }
    }
}
