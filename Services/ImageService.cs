using Microsoft.Extensions.Options;
using PharmaLink.Models;

namespace PharmaLink.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly AppSettings _appSettings;

    public ImageService(IWebHostEnvironment env, IOptions<AppSettings> appSettings)
    {
        _env = env;
        _appSettings = appSettings.Value;
    }

    public async Task<string?> UploadImageAsync(IFormFile file, string folder = "uploads")
    {
        if (file == null || file.Length == 0) return null;
        if (!IsValidImage(file)) return null;

        var uploadsPath = Path.Combine(_env.WebRootPath, folder);
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/{folder}/{fileName}";
    }

    public void DeleteImage(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;

        var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;

        var maxSize = _appSettings.MaxImageSizeMB * 1024 * 1024;
        if (file.Length > maxSize) return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = _appSettings.AllowedImageExtensions.Split(',');
        return allowedExtensions.Contains(extension);
    }
}