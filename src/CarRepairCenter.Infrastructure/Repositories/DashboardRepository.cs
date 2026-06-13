using Microsoft.EntityFrameworkCore;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Core.Enums;
using CarRepairCenter.Infrastructure.Data;

namespace CarRepairCenter.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _db;

    public DashboardRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetTodayOrdersCountAsync(DateTime today)
    {
        return await _db.RepairOrders.CountAsync(r => r.CreatedAt.Date == today);
    }

    public async Task<int> GetActiveOrdersCountAsync()
    {
        return await _db.RepairOrders.CountAsync(r => r.Status == RepairStatus.Waiting || r.Status == RepairStatus.InProgress);
    }

    public async Task<decimal> GetTodayRevenueAsync(DateTime today)
    {
        return await _db.Payments.Where(p => p.PaidAt.Date == today).SumAsync(p => (decimal?)p.Amount) ?? 0m;
    }

    public async Task<int> GetLowStockItemsCountAsync()
    {
        return await _db.InventoryItems.CountAsync(i => i.Quantity <= i.MinStockLevel && i.IsActive);
    }

    public async Task<int> GetTotalCustomersCountAsync()
    {
        return await _db.Customers.CountAsync();
    }

    public async Task<decimal> CalculateOutstandingAsync()
    {
        return await _db.RepairOrders
            .Select(r => new
            {
                ServiceTotal = r.RepairOrderServices.Sum(s => (decimal?)s.Price) ?? 0m,
                PartsTotal = r.RepairOrderParts.Sum(p => (decimal?)(p.Quantity * p.UnitPrice)) ?? 0m,
                Paid = r.Payments.Sum(p => (decimal?)p.Amount) ?? 0m,
                r.DiscountPercentage
            })
            .Select(x => new
            {
                Remaining = (x.ServiceTotal + x.PartsTotal) - ((x.ServiceTotal + x.PartsTotal) * x.DiscountPercentage / 100m) - x.Paid
            })
            .Where(x => x.Remaining > 0)
            .SumAsync(x => (decimal?)x.Remaining) ?? 0m;
    }

    public async Task<List<RepairOrder>> GetRecentOrdersAsync(int count)
    {
        return await _db.RepairOrders
            .Include(r => r.Customer).Include(r => r.Vehicle)
            .Include(r => r.RepairOrderServices).ThenInclude(s => s.Service)
            .Include(r => r.RepairOrderParts).ThenInclude(p => p.InventoryItem)
            .Include(r => r.Payments)
            .AsSplitQuery()
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentsForDateAsync(DateTime date)
    {
        return await _db.Payments.Where(p => p.PaidAt.Date == date).ToListAsync();
    }

    public async Task<int> GetOrdersCreatedCountAsync(DateTime date)
    {
        return await _db.RepairOrders.CountAsync(r => r.CreatedAt.Date == date);
    }

    public async Task<int> GetOrdersCompletedCountAsync(DateTime date)
    {
        return await _db.RepairOrders.CountAsync(r => r.CompletedAt.HasValue && r.CompletedAt.Value.Date == date);
    }
}
