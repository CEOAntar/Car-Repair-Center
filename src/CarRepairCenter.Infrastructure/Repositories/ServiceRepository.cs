using Microsoft.EntityFrameworkCore;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Infrastructure.Data;

namespace CarRepairCenter.Infrastructure.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _db;

    public ServiceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Service>> GetAllAsync(bool? activeOnly)
    {
        var query = _db.Services.AsQueryable();
        if (activeOnly == true) 
            query = query.Where(s => s.IsActive);

        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<Service?> GetByIdAsync(int id)
    {
        return await _db.Services.FindAsync(id);
    }

    public async Task AddAsync(Service service)
    {
        _db.Services.Add(service);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        _db.Services.Update(service);
        await _db.SaveChangesAsync();
    }
}
