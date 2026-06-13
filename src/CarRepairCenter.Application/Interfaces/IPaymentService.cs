using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;

namespace CarRepairCenter.Application.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetAllAsync(int? repairOrderId, DateTime? date, int? customerId, string? search);
    Task<ServiceResult<PaymentDto>> CreateAsync(CreatePaymentDto dto, string? userId);
}
