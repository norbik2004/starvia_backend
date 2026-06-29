using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.PostAttachment.Request
{
    public class PostAttachmentRequest
    {
        public required List<IFormFile> Files { get; set; }
        public required int PostId { get; set; }
    }
}
