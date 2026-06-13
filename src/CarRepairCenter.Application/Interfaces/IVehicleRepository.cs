using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Interfaces;

public interface IVehicleRepository
{
    Task<List<Vehicle>> GetAllAsync(int? customerId, string? search);
    Task<Vehicle?> GetByIdAsync(int id);
    Task AddAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
    Task<List<RepairOrder>> GetHistoryByVehicleIdAsync(int vehicleId);
}
