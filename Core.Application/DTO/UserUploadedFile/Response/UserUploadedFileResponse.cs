using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.UserUploadedFile.Response
{
    public class UserUploadedFileResponse
    {
        public required Guid Id { get; set; }
        public required string FileName { get; set; }
        public required DateTime CreatedAt { get; set; }
        public required string PreviewUrl { get; set; }
    }
}
