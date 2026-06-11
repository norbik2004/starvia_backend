using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.Post.Response;
using Core.Enums;

namespace Core.DTO.PostPublication.Request
{
    public class PublishPostRequest
    {
        public required int PostId {  get; set; }
        public required int UserPlatformId { get; set; }
    }
}
