using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.PostAttachment.Response
{
    public class PostAttachmentResponse
    {
        public required int PostId { get; set; }
        public required Guid UserUploadedFileId { get; set; }
        public required int Order { get; set; }
    }
}
