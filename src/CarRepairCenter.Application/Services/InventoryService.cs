using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepo;

    public InventoryService(IInventoryRepository inventoryRepo)
    {
        _inventoryRepo = inventoryRepo;
    }

    public async Task<List<InventoryItemDto>> GetAllAsync(string? search)
    {
        var items = await _inventoryRepo.GetAllAsync(search);
        return items.Select(i => new InventoryItemDto(
            i.Id, 
            i.ItemCode, 
            i.Name, 
            i.Category, 
            i.Quantity, 
            i.UnitPrice, 
            i.MinStockLevel, 
            i.Unit, 
            i.IsActive, 
            i.Quantity <= i.MinStockLevel
        )).ToList();
    }

    public async Task<ServiceResult<InventoryItemDto>> CreateAsync(CreateInventoryItemDto dto)
    {
        var lastCode = await _inventoryRepo.GetLastItemCodeAsync();
        var nextNum = 1;
        if (lastCode is not null && int.TryParse(lastCode.Replace("INV-", ""), out var n)) 
            nextNum = n + 1;

        var item = new InventoryItem
        {
            ItemCode = $"INV-{nextNum:D4}",
            Name = dto.Name,
            Category = dto.Category,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            MinStockLevel = dto.MinStockLevel,
            Unit = dto.Unit
        };

        await _inventoryRepo.AddAsync(item);

        return ServiceResult<InventoryItemDto>.Success(new InventoryItemDto(
            item.Id, 
            item.ItemCode, 
            item.Name, 
            item.Category, 
            item.Quantity, 
            item.UnitPrice, 
            item.MinStockLevel, 
            item.Unit, 
            item.IsActive, 
            item.IsLowStock
        ));
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, UpdateInventoryItemDto dto)
    {
        var item = await _inventoryRepo.GetByIdAsync(id);
        if (item is null) return ServiceResult<bool>.NotFound();

        item.Name = dto.Name;
        item.Category = dto.Category;
        item.Quantity = dto.Quantity;
        item.UnitPrice = dto.UnitPrice;
        item.MinStockLevel = dto.MinStockLevel;
        item.Unit = dto.Unit;
        item.IsActive = dto.IsActive;

        await _inventoryRepo.UpdateAsync(item);
        return ServiceResult<bool>.Success(true);
    }
}
