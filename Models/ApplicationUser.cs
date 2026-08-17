using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PharmaLink.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public UserProfile? Profile { get; set; }
    public Pharmacy? OwnedPharmacy { get; set; }
    public ICollection<FavoritePharmacy> FavoritePharmacies { get; set; } = new List<FavoritePharmacy>();
    public ICollection<FavoriteMedicine> FavoriteMedicines { get; set; } = new List<FavoriteMedicine>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();

    public string FullName => $"{FirstName} {LastName}";
}