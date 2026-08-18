using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class Pharmacy
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Pharmacy name is required")]
    [StringLength(200)]
    [Display(Name = "Pharmacy Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(255)]
    [Display(Name = "Image")]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    [Display(Name = "Is Open")]
    public bool IsOpen { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 1:1 relationship with ApplicationUser (owner)
    public string? OwnerId { get; set; }
    [ForeignKey("OwnerId")]
    public ApplicationUser? Owner { get; set; }

    // Navigation Properties (1:N)
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}