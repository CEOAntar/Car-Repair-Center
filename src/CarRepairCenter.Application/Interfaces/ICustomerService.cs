using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;

namespace CarRepairCenter.Application.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync(string? search);
    Task<ServiceResult<CustomerDto>> GetByIdAsync(int id);
    Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerDto dto);
    Task<ServiceResult<bool>> UpdateAsync(int id, UpdateCustomerDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
