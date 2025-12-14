using ECommerce.Application.Interfaces.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace ECommerce.Infrastructure.Services;

public class AdvancedFileStorageService : IFileStorageService
{
    private readonly string _uploadsFolder;
    private const int ThumbnailSize = 400;

    public AdvancedFileStorageService()
    {
        _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
        if (!Directory.Exists(_uploadsFolder))
        {
            Directory.CreateDirectory(_uploadsFolder);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        var folderPath = Path.Combine(_uploadsFolder, folder);
        var thumbsPath = Path.Combine(folderPath, "thumbs");
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        if (!Directory.Exists(thumbsPath))
        {
            Directory.CreateDirectory(thumbsPath);
        }

        // Generate unique filename with .webp extension
        var uniqueFileName = $"{Guid.NewGuid()}.webp";
        var fullImagePath = Path.Combine(folderPath, uniqueFileName);
        var thumbnailPath = Path.Combine(thumbsPath, uniqueFileName);

        // Load image
        using var image = await Image.LoadAsync(fileStream);

        // Save full-size WebP (high quality)
        var webpEncoder = new WebpEncoder
        {
            Quality = 85 // High quality for full-size
        };
        await image.SaveAsync(fullImagePath, webpEncoder);

        // Generate and save thumbnail (400px max dimension)
        using var thumbnail = image.Clone(ctx =>
        {
            var size = image.Size;
            int width, height;

            if (size.Width > size.Height)
            {
                width = ThumbnailSize;
                height = (int)((double)size.Height / size.Width * ThumbnailSize);
            }
            else
            {
                height = ThumbnailSize;
                width = (int)((double)size.Width / size.Height * ThumbnailSize);
            }

            ctx.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            });
        });

        var thumbEncoder = new WebpEncoder
        {
            Quality = 75 // Slightly lower quality for thumbnails
        };
        await thumbnail.SaveAsync(thumbnailPath, thumbEncoder);

        // Return relative path for URL
        return $"/uploads/{folder}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
        
        // Delete main file
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        // Delete thumbnail
        var directory = Path.GetDirectoryName(fullPath);
        var fileName = Path.GetFileName(fullPath);
        var thumbPath = Path.Combine(directory!, "thumbs", fileName);
        
        if (File.Exists(thumbPath))
        {
            File.Delete(thumbPath);
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
        // Convert /uploads/products/1/image.webp to /uploads/products/1/thumbs/image.webp
        var parts = filePath.Split('/');
        var fileName = parts[^1];
        var basePath = string.Join('/', parts.Take(parts.Length - 1));
        return $"{basePath}/thumbs/{fileName}";
    }
}
