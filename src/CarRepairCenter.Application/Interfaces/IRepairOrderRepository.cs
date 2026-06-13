using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Interfaces;

public interface IRepairOrderRepository
{
    Task<List<RepairOrder>> GetAllAsync(string? status, string? search, int? customerId);
    Task<RepairOrder?> GetByIdAsync(int id, bool tracking = false);
    Task<List<RepairOrder>> GetByVehicleAsync(int vehicleId);
    Task<string?> GetLastOrderCodeAsync();
    Task AddAsync(RepairOrder order);
    Task UpdateAsync(RepairOrder order);
    Task AddServiceAsync(RepairOrderService service);
    Task RemoveServiceAsync(RepairOrderService service);
    Task AddPartAsync(RepairOrderPart part);
    Task RemovePartAsync(RepairOrderPart part);
}
