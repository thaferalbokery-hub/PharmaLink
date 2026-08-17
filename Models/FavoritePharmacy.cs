using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class FavoritePharmacy
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int PharmacyId { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties - Many-to-Many (Customer <-> Pharmacy)
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey("PharmacyId")]
    public Pharmacy Pharmacy { get; set; } = null!;
}