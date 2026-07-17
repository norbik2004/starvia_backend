using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.ImageGeneration.File
{
    public class ImagePromptFileResponse
    {
        public required string FileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string PreviewUrl { get; set; }
    }
}
