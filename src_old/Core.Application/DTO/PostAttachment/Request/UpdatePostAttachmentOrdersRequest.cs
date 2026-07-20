using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.PostAttachment.Request
{
    public class UpdatePostAttachmentOrdersRequest
    {
        public required int PostId { get; set; }
        public required List<PostAttachmentOrderRequest> Attachments { get; set; }
    }

    public class PostAttachmentOrderRequest
    {
        public required Guid UserUploadedFileId { get; set; }
        public required int Order { get; set; }
    }
}
