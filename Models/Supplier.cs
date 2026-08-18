using System.ComponentModel.DataAnnotations;

namespace PharmaLink.Models;

public class Supplier
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Supplier name is required")]
    [StringLength(200)]
    [Display(Name = "Supplier Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact person is required")]
    [StringLength(100)]
    [Display(Name = "Contact Person")]
    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties (M:N through SupplierMedicine)
    public ICollection<SupplierMedicine> SupplierMedicines { get; set; } = new List<SupplierMedicine>();
}