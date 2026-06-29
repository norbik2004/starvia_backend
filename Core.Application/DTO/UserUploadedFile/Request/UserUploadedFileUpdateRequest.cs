using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.UserUploadedFile.Request
{
    public class UserUploadedFileUpdateRequest
    {
        public Guid Id {  get; set; }

        [MaxLength(50)]
        public required string FileName { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
