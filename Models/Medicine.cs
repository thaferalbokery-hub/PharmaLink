using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class Medicine
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Medicine name is required")]
    [StringLength(300)]
    [Display(Name = "Medicine Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 99999.99, ErrorMessage = "Price must be greater than 0")]
    [Column(TypeName = "decimal(18,2)")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Expiry Date")]
    public DateTime? ExpiryDate { get; set; }

    [StringLength(255)]
    [Display(Name = "Image")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Requires Prescription")]
    public bool RequiresPrescription { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<SupplierMedicine> SupplierMedicines { get; set; } = new List<SupplierMedicine>();
}