using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;

namespace CarRepairCenter.Application.Interfaces;

public interface IInventoryService
{
    Task<List<InventoryItemDto>> GetAllAsync(string? search);
    Task<ServiceResult<InventoryItemDto>> CreateAsync(CreateInventoryItemDto dto);
    Task<ServiceResult<bool>> UpdateAsync(int id, UpdateInventoryItemDto dto);
}
