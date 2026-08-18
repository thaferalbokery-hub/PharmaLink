namespace PharmaLink.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageService> _logger;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
    private const int MaxFileSizeMB = 5;
    private const int MaxFileSizeBytes = MaxFileSizeMB * 1024 * 1024;

    public ImageService(IWebHostEnvironment env, ILogger<ImageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string?> UploadImageAsync(IFormFile file, string folder = "uploads")
    {
        if (file == null || file.Length == 0) return null;
        if (!IsValidImage(file)) return null;

        var uploadsPath = Path.Combine(_env.WebRootPath, folder);
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        // Generate safe unique filename - never trust user-provided filename
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            _logger.LogInformation("Image uploaded: {FileName}", fileName);
            return $"/{folder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return null;
        }
    }

    public void DeleteImage(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;

        try
        {
            var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Image deleted: {ImageUrl}", imageUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
        }
    }

    public bool IsValidImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return false;

        // Validate file size
        if (file.Length > MaxFileSizeBytes) return false;

        // Validate extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension)) return false;

        // Validate MIME type
        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant())) return false;

        return true;
    }
}