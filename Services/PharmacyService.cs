using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Services;

public class PharmacyService : IPharmacyService
{
    private readonly ApplicationDbContext _context;

    public PharmacyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PharmacyListViewModel>> GetAllPharmaciesAsync()
    {
        return await _context.Pharmacies
            .Where(p => p.IsActive)
            .Include(p => p.Inventories)
            .Include(p => p.Reviews)
            .Select(p => new PharmacyListViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                City = p.City,
                Phone = p.Phone,
                IsOpen = p.IsOpen,
                ImageUrl = p.ImageUrl,
                AvailableMedicineCount = p.Inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available),
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0
            })
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<PharmacyDetailsViewModel?> GetPharmacyDetailsAsync(int id, string? userId = null)
    {
        var pharmacy = await _context.Pharmacies
            .Include(p => p.Owner)
            .Include(p => p.WorkingHours)
            .Include(p => p.Contacts)
            .Include(p => p.Reviews).ThenInclude(r => r.User)
            .Include(p => p.Inventories).ThenInclude(i => i.Medicine).ThenInclude(m => m.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pharmacy == null) return null;

        var isFavorited = false;
        if (!string.IsNullOrEmpty(userId))
        {
            isFavorited = await _context.FavoritePharmacies
                .AnyAsync(f => f.UserId == userId && f.PharmacyId == id);
        }

        return new PharmacyDetailsViewModel
        {
            Id = pharmacy.Id,
            Name = pharmacy.Name,
            Description = pharmacy.Description,
            Phone = pharmacy.Phone,
            Email = pharmacy.Email,
            Address = pharmacy.Address,
            City = pharmacy.City,
            IsOpen = pharmacy.IsOpen,
            IsActive = pharmacy.IsActive,
            ImageUrl = pharmacy.ImageUrl,
            OwnerName = pharmacy.Owner.FullName,
            Latitude = pharmacy.Latitude,
            Longitude = pharmacy.Longitude,
            AvailableMedicineCount = pharmacy.Inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available),
            AverageRating = pharmacy.Reviews.Any() ? pharmacy.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = pharmacy.Reviews.Count,
            WorkingHours = pharmacy.WorkingHours.OrderBy(w => w.DayOfWeek).ToList(),
            Contacts = pharmacy.Contacts.ToList(),
            IsFavorited = isFavorited,
            Inventory = pharmacy.Inventories.Select(i => new InventoryItemViewModel
            {
                Id = i.Id,
                MedicineId = i.MedicineId,
                MedicineName = i.Medicine.CommercialName,
                MedicineScientificName = i.Medicine.ScientificName,
                CategoryName = i.Medicine.Category.Name,
                Quantity = i.Quantity,
                Price = i.Price,
                AvailabilityStatus = i.AvailabilityStatus,
                LastUpdated = i.LastUpdated
            }).ToList(),
            Reviews = pharmacy.Reviews.Select(r => new ReviewViewModel
            {
                Id = r.Id,
                UserName = r.User.FullName,
                UserId = r.UserId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                PharmacyId = r.PharmacyId,
                PharmacyName = pharmacy.Name
            }).OrderByDescending(r => r.CreatedAt).ToList()
        };
    }

    public async Task<Pharmacy?> GetPharmacyByIdAsync(int id)
    {
        return await _context.Pharmacies.FindAsync(id);
    }

    public async Task<Pharmacy?> GetPharmacyByOwnerIdAsync(string ownerId)
    {
        return await _context.Pharmacies
            .Include(p => p.WorkingHours)
            .Include(p => p.Contacts)
            .FirstOrDefaultAsync(p => p.OwnerId == ownerId);
    }

    public async Task<Pharmacy> CreatePharmacyAsync(PharmacyCreateViewModel model)
    {
        var pharmacy = new Pharmacy
        {
            Name = model.Name,
            Description = model.Description,
            Phone = model.Phone,
            Email = model.Email,
            Address = model.Address,
            City = model.City,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            OwnerId = model.OwnerId!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Pharmacies.Add(pharmacy);
        await _context.SaveChangesAsync();
        return pharmacy;
    }

    public async Task UpdatePharmacyAsync(PharmacyEditViewModel model)
    {
        var pharmacy = await _context.Pharmacies.FindAsync(model.Id);
        if (pharmacy == null) return;

        pharmacy.Name = model.Name;
        pharmacy.Description = model.Description;
        pharmacy.Phone = model.Phone;
        pharmacy.Email = model.Email;
        pharmacy.Address = model.Address;
        pharmacy.City = model.City;
        pharmacy.Latitude = model.Latitude;
        pharmacy.Longitude = model.Longitude;
        pharmacy.IsOpen = model.IsOpen;
        pharmacy.IsActive = model.IsActive;
        pharmacy.UpdatedAt = DateTime.UtcNow;

        if (model.ImageUrl != null)
            pharmacy.ImageUrl = model.ImageUrl;

        await _context.SaveChangesAsync();
    }

    public async Task DeletePharmacyAsync(int id)
    {
        var pharmacy = await _context.Pharmacies.FindAsync(id);
        if (pharmacy != null)
        {
            _context.Pharmacies.Remove(pharmacy);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleStatusAsync(int id)
    {
        var pharmacy = await _context.Pharmacies.FindAsync(id);
        if (pharmacy != null)
        {
            pharmacy.IsOpen = !pharmacy.IsOpen;
            pharmacy.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PharmacySearchViewModel> SearchPharmaciesAsync(string? searchTerm, string? city, bool? isOpen)
    {
        var query = _context.Pharmacies
            .Where(p => p.IsActive)
            .Include(p => p.Inventories)
            .Include(p => p.Reviews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || p.Address.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(p => p.City == city);
        }

        if (isOpen.HasValue)
        {
            query = query.Where(p => p.IsOpen == isOpen.Value);
        }

        var results = await query.Select(p => new PharmacyListViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Address = p.Address,
            City = p.City,
            Phone = p.Phone,
            IsOpen = p.IsOpen,
            ImageUrl = p.ImageUrl,
            AvailableMedicineCount = p.Inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available),
            AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0
        }).ToListAsync();

        var cities = await GetAllCitiesAsync();

        return new PharmacySearchViewModel
        {
            SearchTerm = searchTerm,
            City = city,
            IsOpen = isOpen,
            Results = results,
            Cities = cities
        };
    }

    public async Task<List<string>> GetAllCitiesAsync()
    {
        return await _context.Pharmacies
            .Where(p => p.IsActive)
            .Select(p => p.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
}