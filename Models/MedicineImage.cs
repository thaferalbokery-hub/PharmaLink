using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class MedicineImage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int MedicineId { get; set; }

    [Required]
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(200)]
    public string? AltText { get; set; }

    public bool IsPrimary { get; set; } = false;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    [ForeignKey("MedicineId")]
    public Medicine Medicine { get; set; } = null!;
}