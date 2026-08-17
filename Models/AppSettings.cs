namespace PharmaLink.Models;

public class AppSettings
{
    public string Currency { get; set; } = "SAR";
    public string CurrencySymbol { get; set; } = "ر.س";
    public int LowStockThreshold { get; set; } = 10;
    public int OutOfStockThreshold { get; set; } = 0;
    public int MaxImageSizeMB { get; set; } = 5;
    public string AllowedImageExtensions { get; set; } = ".jpg,.jpeg,.png,.webp";
}