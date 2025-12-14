namespace ECommerce.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder);
    Task DeleteFileAsync(string filePath);
    bool FileExists(string filePath);
    string GetFileUrl(string filePath);
    string GetThumbnailUrl(string filePath);
}
