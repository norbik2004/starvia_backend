using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.Post.Response;
using Core.Domain.Enums;

namespace Core.Application.DTO.PostPublication.Request
{
    public class PublishPostRequest
    {
        public required int PostId {  get; set; }
        public required int UserPlatformId { get; set; }
    }
}
