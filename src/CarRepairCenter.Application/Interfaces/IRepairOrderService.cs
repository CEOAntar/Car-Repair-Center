using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;

namespace CarRepairCenter.Application.Interfaces;

public interface IRepairOrderService
{
    Task<List<RepairOrderDto>> GetAllAsync(string? status, string? search, int? customerId);
    Task<ServiceResult<RepairOrderDto>> GetByIdAsync(int id);
    Task<List<RepairOrderDto>> GetByVehicleAsync(int vehicleId);
    Task<ServiceResult<RepairOrderDto>> CreateAsync(CreateRepairOrderDto dto, string? userId);
    Task<ServiceResult<bool>> UpdateStatusAsync(int id, UpdateRepairOrderStatusDto dto);
    Task<ServiceResult<bool>> UpdateDiscountAsync(int id, decimal discountPercentage);
    Task<ServiceResult<bool>> AddServiceAsync(int id, AddRepairOrderServiceDto dto);
    Task<ServiceResult<bool>> RemoveServiceAsync(int orderId, int serviceLineId);
    Task<ServiceResult<object?>> AddPartAsync(int id, AddRepairOrderPartDto dto);
    Task<ServiceResult<bool>> RemovePartAsync(int orderId, int partLineId);
}
