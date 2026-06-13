using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Interfaces;

public interface IDashboardRepository
{
    Task<int> GetTodayOrdersCountAsync(DateTime today);
    Task<int> GetActiveOrdersCountAsync();
    Task<decimal> GetTodayRevenueAsync(DateTime today);
    Task<int> GetLowStockItemsCountAsync();
    Task<int> GetTotalCustomersCountAsync();
    Task<decimal> CalculateOutstandingAsync();
    Task<List<RepairOrder>> GetRecentOrdersAsync(int count);
    Task<List<Payment>> GetPaymentsForDateAsync(DateTime date);
    Task<int> GetOrdersCreatedCountAsync(DateTime date);
    Task<int> GetOrdersCompletedCountAsync(DateTime date);
}
