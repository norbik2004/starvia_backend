using Microsoft.AspNetCore.Http;

namespace Core.Application.Services
{
    public interface IMinioService
    {
        Task<bool> SaveFilesAsync(List<(string ObjectPath, IFormFile File)> files, string bucketName);
        Task<MemoryStream> DownloadFileAsync(string bucketName, string filePath);
        Task<bool> DeleteFileAsync(string bucketName, string filePath);
    }
}
