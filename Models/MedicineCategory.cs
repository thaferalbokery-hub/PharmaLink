using System.ComponentModel.DataAnnotations;

namespace PharmaLink.Models;

public class MedicineCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? IconUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}