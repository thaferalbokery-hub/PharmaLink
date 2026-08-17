using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class PharmacyAddress
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PharmacyId { get; set; }

    [Required]
    [StringLength(200)]
    public string Street { get; set; } = string.Empty;

    [StringLength(100)]
    public string? District { get; set; }

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Country { get; set; } = "Saudi Arabia";

    [StringLength(500)]
    public string? AdditionalInfo { get; set; }

    // Navigation Property - One-to-One with Pharmacy
    [ForeignKey("PharmacyId")]
    public Pharmacy Pharmacy { get; set; } = null!;
}