using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;

namespace CarRepairCenter.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepo;

    public DashboardService(IDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;

        var todayOrders = await _dashboardRepo.GetTodayOrdersCountAsync(today);
        var activeOrders = await _dashboardRepo.GetActiveOrdersCountAsync();
        var todayRevenue = await _dashboardRepo.GetTodayRevenueAsync(today);
        var lowStockItems = await _dashboardRepo.GetLowStockItemsCountAsync();
        var totalCustomers = await _dashboardRepo.GetTotalCustomersCountAsync();

        var totalOutstanding = await _dashboardRepo.CalculateOutstandingAsync();

        var recentOrders = await _dashboardRepo.GetRecentOrdersAsync(5);

        return new DashboardDto(
            todayOrders, 
            activeOrders, 
            todayRevenue, 
            totalOutstanding, 
            lowStockItems, 
            totalCustomers,
            recentOrders.Select(r => new RepairOrderDto(
                r.Id, 
                r.OrderCode, 
                r.CustomerId, 
                r.Customer.Name, 
                r.Customer.Phone,
                r.VehicleId, 
                r.Vehicle.PlateNumber, 
                $"{r.Vehicle.Make} {r.Vehicle.Model}",
                r.ProblemDescription, 
                r.Status.ToString(), 
                r.DiscountPercentage, 
                r.EstimatedCost,
                r.TotalServicesAmount, 
                r.TotalPartsAmount, 
                r.SubTotal,
                r.DiscountAmount, 
                r.TotalAmount, 
                r.PaidAmount, 
                r.RemainingAmount, 
                r.IsFullyPaid,
                r.Notes, 
                r.CreatedAt, 
                r.StartedAt, 
                r.CompletedAt, 
                r.DeliveredAt,
                r.RepairOrderServices.Select(s => new RepairOrderServiceDto(s.Id, s.ServiceId, s.Service.Name, s.Price, s.Notes)).ToList(),
                r.RepairOrderParts.Select(p => new RepairOrderPartDto(p.Id, p.InventoryItemId, p.InventoryItem.Name, p.Quantity, p.UnitPrice, p.TotalPrice)).ToList(),
                r.Payments.Select(p => new PaymentDto(p.Id, p.RepairOrderId, r.OrderCode, r.Customer.Name, p.Amount, p.PaymentMethod.ToString(), p.Notes, p.PaidAt)).ToList()
            )).ToList()
        );
    }

    public async Task<ServiceResult<DailyReportDto>> GetDailyReportAsync(DateTime? date)
    {
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;

        var payments = await _dashboardRepo.GetPaymentsForDateAsync(targetDate);
        var ordersCreated = await _dashboardRepo.GetOrdersCreatedCountAsync(targetDate);
        var ordersCompleted = await _dashboardRepo.GetOrdersCompletedCountAsync(targetDate);
        var pendingOrders = await _dashboardRepo.GetActiveOrdersCountAsync();

        var outstanding = await _dashboardRepo.CalculateOutstandingAsync();

        var breakdown = payments.GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodSummaryDto(g.Key.ToString(), g.Sum(p => p.Amount), g.Count()))
            .ToList();

        return ServiceResult<DailyReportDto>.Success(
            new DailyReportDto(
                targetDate, 
                payments.Sum(p => p.Amount), 
                ordersCreated, 
                ordersCompleted, 
                pendingOrders, 
                outstanding, 
                breakdown
            )
        );
    }
}
