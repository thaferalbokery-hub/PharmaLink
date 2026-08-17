using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaLink.Models;

public class PharmacyContact
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PharmacyId { get; set; }

    [Required]
    [StringLength(50)]
    public string ContactType { get; set; } = string.Empty; // Phone, Email, WhatsApp, etc.

    [Required]
    [StringLength(200)]
    public string ContactValue { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Label { get; set; }

    // Navigation Property
    [ForeignKey("PharmacyId")]
    public Pharmacy Pharmacy { get; set; } = null!;
}