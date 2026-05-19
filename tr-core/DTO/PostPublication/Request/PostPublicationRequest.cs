using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Post.Response;
using tr_core.Enums;

namespace tr_core.DTO.PostPublication.Request
{
    public class PostPublicationRequest
    {
        public required int PostId {  get; set; }
        public required int UserPlatformId { get; set; }
        public required PostPublicationStatus Status { get; set; }
    }
}
