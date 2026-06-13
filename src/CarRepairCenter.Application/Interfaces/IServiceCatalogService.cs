using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;

namespace CarRepairCenter.Application.Interfaces;

public interface IServiceCatalogService
{
    Task<List<ServiceDto>> GetAllAsync(bool? activeOnly);
    Task<ServiceResult<ServiceDto>> CreateAsync(CreateServiceDto dto);
    Task<ServiceResult<bool>> UpdateAsync(int id, UpdateServiceDto dto);
}
