using Microsoft.EntityFrameworkCore;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Core.Enums;
using CarRepairCenter.Infrastructure.Data;

namespace CarRepairCenter.Infrastructure.Repositories;

public class RepairOrderRepository : IRepairOrderRepository
{
    private readonly AppDbContext _db;

    public RepairOrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RepairOrder>> GetAllAsync(string? status, string? search, int? customerId)
    {
        var query = _db.RepairOrders
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .Include(r => r.RepairOrderServices).ThenInclude(s => s.Service)
            .Include(r => r.RepairOrderParts).ThenInclude(p => p.InventoryItem)
            .Include(r => r.Payments)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<RepairStatus>(status, true, out var s))
            query = query.Where(r => r.Status == s);
        if (customerId.HasValue)
            query = query.Where(r => r.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.OrderCode.Contains(search) 
                || r.Customer.Name.Contains(search) 
                || r.Customer.CustomerCode.Contains(search));
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<RepairOrder?> GetByIdAsync(int id, bool tracking = false)
    {
        var query = _db.RepairOrders
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .Include(r => r.RepairOrderServices).ThenInclude(s => s.Service)
            .Include(r => r.RepairOrderParts).ThenInclude(p => p.InventoryItem)
            .Include(r => r.Payments)
            .AsSplitQuery()
            .AsQueryable();

        if (!tracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<RepairOrder>> GetByVehicleAsync(int vehicleId)
    {
        return await _db.RepairOrders
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .Include(r => r.RepairOrderServices).ThenInclude(s => s.Service)
            .Include(r => r.RepairOrderParts).ThenInclude(p => p.InventoryItem)
            .Include(r => r.Payments)
            .AsSplitQuery()
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<string?> GetLastOrderCodeAsync()
    {
        return await _db.RepairOrders
            .OrderByDescending(r => r.Id)
            .Select(r => r.OrderCode)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(RepairOrder order)
    {
        _db.RepairOrders.Add(order);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(RepairOrder order)
    {
        _db.RepairOrders.Update(order);
        await _db.SaveChangesAsync();
    }

    public async Task AddServiceAsync(RepairOrderService service)
    {
        _db.RepairOrderServices.Add(service);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveServiceAsync(RepairOrderService service)
    {
        _db.RepairOrderServices.Remove(service);
        await _db.SaveChangesAsync();
    }

    public async Task AddPartAsync(RepairOrderPart part)
    {
        _db.RepairOrderParts.Add(part);
        await _db.SaveChangesAsync();
    }

    public async Task RemovePartAsync(RepairOrderPart part)
    {
        _db.RepairOrderParts.Remove(part);
        await _db.SaveChangesAsync();
    }
}
