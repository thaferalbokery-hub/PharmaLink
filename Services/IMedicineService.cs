using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Services;

public interface IMedicineService
{
    Task<List<MedicineListViewModel>> GetAllMedicinesAsync();
    Task<MedicineDetailsViewModel?> GetMedicineDetailsAsync(int id, string? userId = null);
    Task<Medicine?> GetMedicineByIdAsync(int id);
    Task<Medicine> CreateMedicineAsync(MedicineCreateViewModel model);
    Task UpdateMedicineAsync(MedicineEditViewModel model);
    Task DeleteMedicineAsync(int id);
    Task<MedicineSearchViewModel> SearchMedicinesAsync(string? searchTerm, int? categoryId, int? brandId, AvailabilityStatus? availability);
    Task<List<MedicineCategory>> GetAllCategoriesAsync();
    Task<List<MedicineBrand>> GetAllBrandsAsync();
}