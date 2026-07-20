using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.UserUploadedFile.Request
{
    public class UserUploadedFileRequest
    {
        public required List<IFormFile> Files { get; set; }
    }
}
