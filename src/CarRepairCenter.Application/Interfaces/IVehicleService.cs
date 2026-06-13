using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;

namespace CarRepairCenter.Application.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleDto>> GetAllAsync(int? customerId, string? search);
    Task<ServiceResult<VehicleDto>> GetByIdAsync(int id);
    Task<ServiceResult<VehicleDto>> CreateAsync(CreateVehicleDto dto);
    Task<ServiceResult<bool>> UpdateAsync(int id, CreateVehicleDto dto);
    Task<ServiceResult<object>> GetHistoryAsync(int id);
}
