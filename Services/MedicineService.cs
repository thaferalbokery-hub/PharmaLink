using Microsoft.EntityFrameworkCore;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Services;

public class MedicineService : IMedicineService
{
    private readonly ApplicationDbContext _context;

    public MedicineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MedicineListViewModel>> GetAllMedicinesAsync()
    {
        return await _context.Medicines
            .Where(m => m.IsActive)
            .Include(m => m.Category)
            .Include(m => m.Brand)
            .Include(m => m.Images)
            .Include(m => m.Inventories)
            .Select(m => new MedicineListViewModel
            {
                Id = m.Id,
                ScientificName = m.ScientificName,
                CommercialName = m.CommercialName,
                CategoryName = m.Category.Name,
                BrandName = m.Brand != null ? m.Brand.Name : null,
                DosageForm = m.DosageForm,
                Strength = m.Strength,
                PrimaryImageUrl = m.Images.FirstOrDefault(i => i.IsPrimary) != null
                    ? m.Images.First(i => i.IsPrimary).ImageUrl
                    : m.Images.FirstOrDefault() != null ? m.Images.First().ImageUrl : null,
                RequiresPrescription = m.RequiresPrescription,
                PharmacyCount = m.Inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available),
                MinPrice = m.Inventories.Any() ? m.Inventories.Min(i => i.Price) : null,
                MaxPrice = m.Inventories.Any() ? m.Inventories.Max(i => i.Price) : null
            })
            .OrderBy(m => m.CommercialName)
            .ToListAsync();
    }

    public async Task<MedicineDetailsViewModel?> GetMedicineDetailsAsync(int id, string? userId = null)
    {
        var medicine = await _context.Medicines
            .Include(m => m.Category)
            .Include(m => m.Brand)
            .Include(m => m.Images)
            .Include(m => m.Inventories).ThenInclude(i => i.Pharmacy)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medicine == null) return null;

        var isFavorited = false;
        if (!string.IsNullOrEmpty(userId))
        {
            isFavorited = await _context.FavoriteMedicines
                .AnyAsync(f => f.UserId == userId && f.MedicineId == id);
        }

        return new MedicineDetailsViewModel
        {
            Id = medicine.Id,
            ScientificName = medicine.ScientificName,
            CommercialName = medicine.CommercialName,
            Description = medicine.Description,
            CategoryName = medicine.Category.Name,
            BrandName = medicine.Brand?.Name,
            DosageForm = medicine.DosageForm,
            Strength = medicine.Strength,
            Unit = medicine.Unit,
            RequiresPrescription = medicine.RequiresPrescription,
            Images = medicine.Images.ToList(),
            IsFavorited = isFavorited,
            PharmacyAvailabilities = medicine.Inventories
                .Where(i => i.Pharmacy.IsActive)
                .Select(i => new PharmacyAvailabilityViewModel
                {
                    PharmacyId = i.PharmacyId,
                    PharmacyName = i.Pharmacy.Name,
                    PharmacyCity = i.Pharmacy.City,
                    PharmacyIsOpen = i.Pharmacy.IsOpen,
                    Price = i.Price,
                    AvailabilityStatus = i.AvailabilityStatus,
                    LastUpdated = i.LastUpdated
                }).ToList()
        };
    }

    public async Task<Medicine?> GetMedicineByIdAsync(int id)
    {
        return await _context.Medicines
            .Include(m => m.Images)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Medicine> CreateMedicineAsync(MedicineCreateViewModel model)
    {
        var medicine = new Medicine
        {
            ScientificName = model.ScientificName,
            CommercialName = model.CommercialName,
            Description = model.Description,
            CategoryId = model.CategoryId,
            BrandId = model.BrandId,
            DosageForm = model.DosageForm,
            Strength = model.Strength,
            Unit = model.Unit,
            RequiresPrescription = model.RequiresPrescription,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();
        return medicine;
    }

    public async Task UpdateMedicineAsync(MedicineEditViewModel model)
    {
        var medicine = await _context.Medicines.FindAsync(model.Id);
        if (medicine == null) return;

        medicine.ScientificName = model.ScientificName;
        medicine.CommercialName = model.CommercialName;
        medicine.Description = model.Description;
        medicine.CategoryId = model.CategoryId;
        medicine.BrandId = model.BrandId;
        medicine.DosageForm = model.DosageForm;
        medicine.Strength = model.Strength;
        medicine.Unit = model.Unit;
        medicine.RequiresPrescription = model.RequiresPrescription;
        medicine.IsActive = model.IsActive;
        medicine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteMedicineAsync(int id)
    {
        var medicine = await _context.Medicines.FindAsync(id);
        if (medicine != null)
        {
            medicine.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<MedicineSearchViewModel> SearchMedicinesAsync(string? searchTerm, int? categoryId, int? brandId, AvailabilityStatus? availability)
    {
        var query = _context.Medicines
            .Where(m => m.IsActive)
            .Include(m => m.Category)
            .Include(m => m.Brand)
            .Include(m => m.Images)
            .Include(m => m.Inventories)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(m =>
                m.ScientificName.Contains(searchTerm) ||
                m.CommercialName.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            query = query.Where(m => m.BrandId == brandId.Value);
        }

        var results = await query.Select(m => new MedicineListViewModel
        {
            Id = m.Id,
            ScientificName = m.ScientificName,
            CommercialName = m.CommercialName,
            CategoryName = m.Category.Name,
            BrandName = m.Brand != null ? m.Brand.Name : null,
            DosageForm = m.DosageForm,
            Strength = m.Strength,
            PrimaryImageUrl = m.Images.FirstOrDefault(i => i.IsPrimary) != null
                ? m.Images.First(i => i.IsPrimary).ImageUrl
                : m.Images.FirstOrDefault() != null ? m.Images.First().ImageUrl : null,
            RequiresPrescription = m.RequiresPrescription,
            PharmacyCount = m.Inventories.Count(i => i.AvailabilityStatus == AvailabilityStatus.Available),
            MinPrice = m.Inventories.Any() ? m.Inventories.Min(i => i.Price) : null,
            MaxPrice = m.Inventories.Any() ? m.Inventories.Max(i => i.Price) : null
        }).ToListAsync();

        if (availability.HasValue)
        {
            var medicineIds = await _context.Inventories
                .Where(i => i.AvailabilityStatus == availability.Value)
                .Select(i => i.MedicineId)
                .Distinct()
                .ToListAsync();
            results = results.Where(r => medicineIds.Contains(r.Id)).ToList();
        }

        return new MedicineSearchViewModel
        {
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            BrandId = brandId,
            Availability = availability,
            Results = results,
            Categories = await GetAllCategoriesAsync(),
            Brands = await GetAllBrandsAsync()
        };
    }

    public async Task<List<MedicineCategory>> GetAllCategoriesAsync()
    {
        return await _context.MedicineCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<MedicineBrand>> GetAllBrandsAsync()
    {
        return await _context.MedicineBrands
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }
}