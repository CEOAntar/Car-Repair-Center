using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Interfaces;

public interface IInventoryRepository
{
    Task<List<InventoryItem>> GetAllAsync(string? search);
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<string?> GetLastItemCodeAsync();
    Task AddAsync(InventoryItem item);
    Task UpdateAsync(InventoryItem item);
}
