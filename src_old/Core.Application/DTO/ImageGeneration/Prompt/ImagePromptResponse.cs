using Core.Application.DTO.ImageGeneration.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.ImageGeneration.Prompt
{
    public class ImagePromptResponse
    {
        public required string Prompt { get; set; }
        public required ImagePromptFileResponse ImagePromptFile { get; set; }
    }
}
