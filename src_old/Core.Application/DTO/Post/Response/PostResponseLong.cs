using Core.Application.DTO.PostAttachment.Response;
using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.Post.Response
{
    public class PostResponseLong
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string UserId { get; set; }
        public string? Body { get; set; }
        public PostStatus Status { get; set; }
        public List<PlatformType>? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PostAttachmentResponse> Attachments { get; set; } = [];
    }
}
