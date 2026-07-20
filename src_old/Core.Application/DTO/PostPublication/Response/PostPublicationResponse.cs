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
        public string PostBody { get; set; }
        public PlatformType PlatformType { get; set; }
        public string AccountUsername { get; set; }
        public PostPublicationStatus Status {  get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? ExternalPostId { get; set; }
    }
}
