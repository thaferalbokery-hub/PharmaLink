using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class Pharmacy
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsOpen { get; set; } = false;

    public bool IsActive { get; set; } = true;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    // Navigation Properties
    [ForeignKey("OwnerId")]
    public ApplicationUser Owner { get; set; } = null!;

    public PharmacyAddress? PharmacyAddress { get; set; }
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<PharmacyWorkingHour> WorkingHours { get; set; } = new List<PharmacyWorkingHour>();
    public ICollection<PharmacyContact> Contacts { get; set; } = new List<PharmacyContact>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<FavoritePharmacy> FavoritedBy { get; set; } = new List<FavoritePharmacy>();
}