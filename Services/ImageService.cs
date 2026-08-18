namespace PharmaLink.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const int MaxFileSizeMB = 5;

    public ImageService(IWebHostEnvironment env)
    {
        _env = env;
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
            File.Delete(filePath);
    }

    public bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;
        if (file.Length > MaxFileSizeMB * 1024 * 1024) return false;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }
}