using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class FavoriteMedicine
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int MedicineId { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties - Many-to-Many (Customer <-> Medicine)
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey("MedicineId")]
    public Medicine Medicine { get; set; } = null!;
}