using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Services;

public interface IInventoryService
{
    Task<List<InventoryItemViewModel>> GetPharmacyInventoryAsync(int pharmacyId);
    Task<Inventory?> GetInventoryItemAsync(int id);
    Task AddInventoryAsync(InventoryAddViewModel model, string userId);
    Task UpdateInventoryAsync(InventoryUpdateViewModel model, string userId);
    Task RemoveInventoryAsync(int id, string userId);
    Task<AvailabilityStatus> CalculateAvailabilityStatus(int quantity, int lowStockThreshold);
}