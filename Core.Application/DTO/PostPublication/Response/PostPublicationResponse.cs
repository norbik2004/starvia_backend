using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Enums;

namespace Core.Application.DTO.PostPublication.Response
{
    public class PostPublicationResponse
    {
        public int PostId { get; set; }
        public int UserPlatformId { get; set; }
        public PostPublicationStatus Status {  get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? ExternalPostId { get; set; }
    }
}
