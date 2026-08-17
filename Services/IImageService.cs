namespace PharmaLink.Services;

public interface IImageService
{
    Task<string?> UploadImageAsync(IFormFile file, string folder = "uploads");
    void DeleteImage(string? imageUrl);
    bool IsValidImage(IFormFile file);
}