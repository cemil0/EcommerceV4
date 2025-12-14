using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsFolder;

    public LocalFileStorageService()
    {
        // Use relative path from API project's wwwroot
        _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
        // Ensure uploads directory exists
        if (!Directory.Exists(_uploadsFolder))
        {
            Directory.CreateDirectory(_uploadsFolder);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        var folderPath = Path.Combine(_uploadsFolder, folder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate unique filename
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        // Return relative path for URL
        return $"/uploads/{folder}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public bool FileExists(string filePath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
        return File.Exists(fullPath);
    }

    public string GetFileUrl(string filePath)
    {
        return filePath; // Already in URL format
    }

    public string GetThumbnailUrl(string filePath)
    {
        // LocalFileStorageService doesn't generate thumbnails, return same URL
        return filePath;
    }
}

