using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PharmaLink.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // 1:1 relationship - A pharmacist user owns one pharmacy
    public int? PharmacyId { get; set; }
    public Pharmacy? OwnedPharmacy { get; set; }

    // Navigation Properties (1:N)
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";
}