using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PharmaLink.Data;
using PharmaLink.Models;
using PharmaLink.ViewModels;

namespace PharmaLink.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly AppSettings _appSettings;

    public InventoryService(ApplicationDbContext context, IOptions<AppSettings> appSettings)
    {
        _context = context;
        _appSettings = appSettings.Value;
    }

    public async Task<List<InventoryItemViewModel>> GetPharmacyInventoryAsync(int pharmacyId)
    {
        return await _context.Inventories
            .Where(i => i.PharmacyId == pharmacyId)
            .Include(i => i.Medicine).ThenInclude(m => m.Category)
            .Select(i => new InventoryItemViewModel
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
            })
            .OrderBy(i => i.MedicineName)
            .ToListAsync();
    }

    public async Task<Inventory?> GetInventoryItemAsync(int id)
    {
        return await _context.Inventories
            .Include(i => i.Medicine)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task AddInventoryAsync(InventoryAddViewModel model, string userId)
    {
        var existing = await _context.Inventories
            .FirstOrDefaultAsync(i => i.PharmacyId == model.PharmacyId && i.MedicineId == model.MedicineId);

        if (existing != null)
        {
            existing.Quantity += model.Quantity;
            existing.Price = model.Price;
            existing.AvailabilityStatus = await CalculateAvailabilityStatus(existing.Quantity, _appSettings.LowStockThreshold);
            existing.LastUpdated = DateTime.UtcNow;
        }
        else
        {
            var status = await CalculateAvailabilityStatus(model.Quantity, _appSettings.LowStockThreshold);
            var inventory = new Inventory
            {
                PharmacyId = model.PharmacyId,
                MedicineId = model.MedicineId,
                Quantity = model.Quantity,
                Price = model.Price,
                AvailabilityStatus = status,
                LastUpdated = DateTime.UtcNow
            };
            _context.Inventories.Add(inventory);
        }

        // Record transaction
        var transaction = new InventoryTransaction
        {
            PharmacyId = model.PharmacyId,
            MedicineId = model.MedicineId,
            Quantity = model.Quantity,
            TransactionType = TransactionType.StockAdded,
            TransactionDate = DateTime.UtcNow,
            UserId = userId,
            Notes = "Stock added"
        };
        _context.InventoryTransactions.Add(transaction);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateInventoryAsync(InventoryUpdateViewModel model, string userId)
    {
        var inventory = await _context.Inventories.FindAsync(model.Id);
        if (inventory == null) return;

        var oldQuantity = inventory.Quantity;
        inventory.Quantity = model.Quantity;
        inventory.Price = model.Price;
        inventory.AvailabilityStatus = await CalculateAvailabilityStatus(model.Quantity, _appSettings.LowStockThreshold);
        inventory.LastUpdated = DateTime.UtcNow;

        // Record transaction
        var quantityDiff = model.Quantity - oldQuantity;
        var transactionType = quantityDiff >= 0 ? TransactionType.StockAdded : TransactionType.StockRemoved;
        var transaction = new InventoryTransaction
        {
            PharmacyId = inventory.PharmacyId,
            MedicineId = inventory.MedicineId,
            Quantity = Math.Abs(quantityDiff),
            TransactionType = transactionType,
            TransactionDate = DateTime.UtcNow,
            UserId = userId,
            Notes = model.Notes ?? "Stock updated"
        };
        _context.InventoryTransactions.Add(transaction);

        await _context.SaveChangesAsync();
    }

    public async Task RemoveInventoryAsync(int id, string userId)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory == null) return;

        var transaction = new InventoryTransaction
        {
            PharmacyId = inventory.PharmacyId,
            MedicineId = inventory.MedicineId,
            Quantity = inventory.Quantity,
            TransactionType = TransactionType.StockRemoved,
            TransactionDate = DateTime.UtcNow,
            UserId = userId,
            Notes = "Item removed from inventory"
        };
        _context.InventoryTransactions.Add(transaction);

        _context.Inventories.Remove(inventory);
        await _context.SaveChangesAsync();
    }

    public Task<AvailabilityStatus> CalculateAvailabilityStatus(int quantity, int lowStockThreshold)
    {
        if (quantity <= 0)
            return Task.FromResult(AvailabilityStatus.OutOfStock);
        if (quantity <= lowStockThreshold)
            return Task.FromResult(AvailabilityStatus.LowStock);
        return Task.FromResult(AvailabilityStatus.Available);
    }
}