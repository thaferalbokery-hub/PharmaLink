using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Services;

public interface IPharmacyService
{
    Task<List<PharmacyListViewModel>> GetAllPharmaciesAsync();
    Task<PharmacyDetailsViewModel?> GetPharmacyDetailsAsync(int id, string? userId = null);
    Task<Pharmacy?> GetPharmacyByIdAsync(int id);
    Task<Pharmacy?> GetPharmacyByOwnerIdAsync(string ownerId);
    Task<Pharmacy> CreatePharmacyAsync(PharmacyCreateViewModel model);
    Task UpdatePharmacyAsync(PharmacyEditViewModel model);
    Task DeletePharmacyAsync(int id);
    Task ToggleStatusAsync(int id);
    Task<PharmacySearchViewModel> SearchPharmaciesAsync(string? searchTerm, string? city, bool? isOpen);
    Task<List<string>> GetAllCitiesAsync();
}