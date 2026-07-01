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
        public required int PostId { get; set; }
        public required List<FileAttachmentRequest> Attachemnts { get; set; }
        
    }

    public class FileAttachmentRequest
    {
        public required Guid UploadedFileId { get; set; }
        public required int Order { get; set; }
    }

}
