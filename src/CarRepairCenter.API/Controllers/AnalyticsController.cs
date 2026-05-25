using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRepairCenter.Core.Enums;
using CarRepairCenter.Infrastructure.Data;

namespace CarRepairCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AnalyticsController(AppDbContext db) => _db = db;

    /// <summary>
    /// KPI summary: Avg order value, avg completion time, collection rate, new customers this month
    /// </summary>
    [HttpGet("kpis")]
    public async Task<ActionResult> GetKpis()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfPrevMonth = startOfMonth.AddMonths(-1);

        // Average Order Value (total amount / number of orders that have services/parts)
        var orderValues = await _db.RepairOrders
            .Select(r => new
            {
                ServiceTotal = r.RepairOrderServices.Sum(s => (decimal?)s.Price) ?? 0m,
                PartsTotal = r.RepairOrderParts.Sum(p => (decimal?)(p.Quantity * p.UnitPrice)) ?? 0m,
                r.DiscountPercentage
            })
            .Where(x => (x.ServiceTotal + x.PartsTotal) > 0)
            .Select(x => (x.ServiceTotal + x.PartsTotal) - ((x.ServiceTotal + x.PartsTotal) * x.DiscountPercentage / 100m))
            .ToListAsync();

        var avgOrderValue = orderValues.Count > 0 ? orderValues.Average() : 0m;

        // Average Completion Time (hours) — from CreatedAt to CompletedAt for completed orders
        var completionTimes = await _db.RepairOrders
            .Where(r => r.CompletedAt.HasValue)
            .Select(r => new { r.CreatedAt, CompletedAt = r.CompletedAt!.Value })
            .ToListAsync();

        var avgCompletionHours = completionTimes.Count > 0
            ? completionTimes.Average(x => (x.CompletedAt - x.CreatedAt).TotalHours)
            : 0;

        // Collection Rate (total paid / total billed * 100)
        var financials = await _db.RepairOrders
            .Select(r => new
            {
                ServiceTotal = r.RepairOrderServices.Sum(s => (decimal?)s.Price) ?? 0m,
                PartsTotal = r.RepairOrderParts.Sum(p => (decimal?)(p.Quantity * p.UnitPrice)) ?? 0m,
                Paid = r.Payments.Sum(p => (decimal?)p.Amount) ?? 0m,
                r.DiscountPercentage
            })
            .Select(x => new
            {
                Billed = (x.ServiceTotal + x.PartsTotal) - ((x.ServiceTotal + x.PartsTotal) * x.DiscountPercentage / 100m),
                x.Paid
            })
            .ToListAsync();

        var totalBilled = financials.Sum(x => x.Billed);
        var totalPaid = financials.Sum(x => x.Paid);
        var collectionRate = totalBilled > 0 ? Math.Round((double)(totalPaid / totalBilled * 100m), 1) : 100;

        // New customers this month
        var newCustomersThisMonth = await _db.Customers
            .CountAsync(c => c.CreatedAt >= startOfMonth);

        // Total orders this month vs last month (for comparison)
        var ordersThisMonth = await _db.RepairOrders.CountAsync(r => r.CreatedAt >= startOfMonth);
        var ordersLastMonth = await _db.RepairOrders.CountAsync(r => r.CreatedAt >= startOfPrevMonth && r.CreatedAt < startOfMonth);

        // Revenue this month vs last month
        var revenueThisMonth = await _db.Payments
            .Where(p => p.PaidAt >= startOfMonth)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var revenueLastMonth = await _db.Payments
            .Where(p => p.PaidAt >= startOfPrevMonth && p.PaidAt < startOfMonth)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        return Ok(new
        {
            avgOrderValue = Math.Round(avgOrderValue, 0),
            avgCompletionHours = Math.Round(avgCompletionHours, 1),
            collectionRate,
            newCustomersThisMonth,
            ordersThisMonth,
            ordersLastMonth,
            revenueThisMonth,
            revenueLastMonth
        });
    }

    /// <summary>
    /// Daily revenue for the last N days (default 30)
    /// </summary>
    [HttpGet("revenue-trend")]
    public async Task<ActionResult> GetRevenueTrend([FromQuery] int days = 30)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        var payments = await _db.Payments
            .Where(p => p.PaidAt >= startDate)
            .GroupBy(p => p.PaidAt.Date)
            .Select(g => new
            {
                date = g.Key,
                revenue = g.Sum(p => p.Amount),
                count = g.Count()
            })
            .OrderBy(x => x.date)
            .ToListAsync();

        // Fill gaps (days with 0 revenue)
        var result = new List<object>();
        for (var d = startDate; d <= DateTime.UtcNow.Date; d = d.AddDays(1))
        {
            var entry = payments.FirstOrDefault(p => p.date == d);
            result.Add(new
            {
                date = d.ToString("MM/dd"),
                dateFull = d.ToString("yyyy-MM-dd"),
                revenue = entry?.revenue ?? 0m,
                count = entry?.count ?? 0
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Payment methods distribution for a period (default last 30 days)
    /// </summary>
    [HttpGet("payment-distribution")]
    public async Task<ActionResult> GetPaymentDistribution([FromQuery] int days = 30)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        var dbBreakdown = await _db.Payments
            .Where(p => p.PaidAt >= startDate)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new
            {
                methodEnum = g.Key,
                amount = g.Sum(p => p.Amount),
                count = g.Count()
            })
            .ToListAsync();

        var totalAmount = dbBreakdown.Sum(x => x.amount);

        var result = dbBreakdown
            .OrderByDescending(x => x.amount)
            .Select(b => new
            {
                method = b.methodEnum.ToString(),
                b.amount,
                b.count,
                percentage = totalAmount > 0 ? Math.Round((double)(b.amount / totalAmount * 100m), 1) : 0
            });

        return Ok(result);
    }

    /// <summary>
    /// Top N most requested services
    /// </summary>
    [HttpGet("top-services")]
    public async Task<ActionResult> GetTopServices([FromQuery] int limit = 10)
    {
        var services = await _db.RepairOrderServices
            .Include(ros => ros.Service)
            .GroupBy(ros => new { ros.ServiceId, ros.Service.Name })
            .Select(g => new
            {
                serviceName = g.Key.Name,
                timesUsed = g.Count(),
                totalRevenue = g.Sum(x => x.Price)
            })
            .OrderByDescending(x => x.timesUsed)
            .Take(limit)
            .ToListAsync();

        return Ok(services);
    }

    /// <summary>
    /// Orders status summary (count per status)
    /// </summary>
    [HttpGet("orders-status")]
    public async Task<ActionResult> GetOrdersStatus()
    {
        var dbStatuses = await _db.RepairOrders
            .GroupBy(r => r.Status)
            .Select(g => new
            {
                statusEnum = g.Key,
                count = g.Count()
            })
            .ToListAsync();

        var statuses = dbStatuses.Select(s => new
        {
            status = s.statusEnum.ToString(),
            s.count
        });

        return Ok(statuses);
    }
}
