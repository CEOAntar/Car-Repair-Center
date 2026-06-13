using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Interfaces;

public interface IPaymentRepository
{
    Task<List<Payment>> GetAllAsync(int? repairOrderId, DateTime? date, int? customerId, string? search);
    Task AddAsync(Payment payment);
}
