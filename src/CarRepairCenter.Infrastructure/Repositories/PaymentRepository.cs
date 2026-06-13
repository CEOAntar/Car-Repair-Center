using Microsoft.EntityFrameworkCore;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Infrastructure.Data;

namespace CarRepairCenter.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;

    public PaymentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Payment>> GetAllAsync(int? repairOrderId, DateTime? date, int? customerId, string? search)
    {
        var query = _db.Payments.Include(p => p.RepairOrder).ThenInclude(r => r.Customer).AsQueryable();
        if (repairOrderId.HasValue) 
            query = query.Where(p => p.RepairOrderId == repairOrderId.Value);
        if (customerId.HasValue) 
            query = query.Where(p => p.RepairOrder.CustomerId == customerId.Value);
        if (date.HasValue) 
            query = query.Where(p => p.PaidAt.Date == date.Value.Date);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.RepairOrder.Customer.Name.Contains(search) 
                || p.RepairOrder.Customer.CustomerCode.Contains(search) 
                || p.RepairOrder.OrderCode.Contains(search));
        }

        return await query.OrderByDescending(p => p.PaidAt).ToListAsync();
    }

    public async Task AddAsync(Payment payment)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
    }
}
