using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Enums;

namespace Core.Application.DTO.Post.Request
{
    public class PostRequest
    {
        [MaxLength(75)]
        public required string Title { get; set; }
        [MaxLength(1000)]
        public string? Body { get; set; }
        public List<PlatformType>? Tags { get; set; }
        public PostStatus Status { get; set; }
    }
}
