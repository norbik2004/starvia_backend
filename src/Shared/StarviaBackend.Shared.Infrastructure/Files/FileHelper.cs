
namespace StarviaBackend.Shared.Infrastructure.Files;

public static class FileHelper
{
    public static string BuildStorageKey(string userEmail, string fileName, Guid fileId)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return $"{userEmail}/{name}_{fileId:N}{extension}";
    }
}
