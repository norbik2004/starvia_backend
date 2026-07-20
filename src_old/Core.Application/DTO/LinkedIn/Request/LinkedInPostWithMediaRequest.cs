using Core.Application.DTO.PostAttachment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.LinkedIn.Request
{
    public class LinkedInPostWithMediaRequest
    {
        public required string AccessToken { get; set; }
        public required string ExternalAccountId { get; set; }
        public required string Content { get; set; }
        public List<PostAttachmentResponse> Attachments { get; set; } = [];
    }
}
