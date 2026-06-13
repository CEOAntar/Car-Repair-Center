using Microsoft.EntityFrameworkCore;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Infrastructure.Data;

namespace CarRepairCenter.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _db;

    public VehicleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Vehicle>> GetAllAsync(int? customerId, string? search)
    {
        var query = _db.Vehicles.AsNoTracking().Include(v => v.Customer).AsQueryable();
        if (customerId.HasValue)
        {
            query = query.Where(v => v.CustomerId == customerId.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.Customer.Name.Contains(search) 
                || v.Customer.CustomerCode.Contains(search) 
                || v.Make.Contains(search) 
                || v.Model.Contains(search));
        }

        return await query.OrderByDescending(v => v.CreatedAt).ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _db.Vehicles.AsNoTracking()
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task AddAsync(Vehicle vehicle)
    {
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        _db.Vehicles.Update(vehicle);
        await _db.SaveChangesAsync();
    }

    public async Task<List<RepairOrder>> GetHistoryByVehicleIdAsync(int vehicleId)
    {
        return await _db.RepairOrders
            .AsNoTracking()
            .Include(r => r.RepairOrderServices)
            .Include(r => r.RepairOrderParts)
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
