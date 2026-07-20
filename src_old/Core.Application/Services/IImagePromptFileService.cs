using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Services
{
    public interface IImagePromptFileService
    {
        public Task<FileStreamResult> DownloadFileById(Guid fileId, string userId);
    }
}
