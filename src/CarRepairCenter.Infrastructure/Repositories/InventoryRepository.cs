using Microsoft.EntityFrameworkCore;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Infrastructure.Data;

namespace CarRepairCenter.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _db;

    public InventoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<InventoryItem>> GetAllAsync(string? search)
    {
        var query = _db.InventoryItems.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search) || i.ItemCode.Contains(search));
        }

        return await query.OrderBy(i => i.Name).ToListAsync();
    }

    public async Task<InventoryItem?> GetByIdAsync(int id)
    {
        return await _db.InventoryItems.FindAsync(id);
    }

    public async Task<string?> GetLastItemCodeAsync()
    {
        return await _db.InventoryItems
            .OrderByDescending(i => i.Id)
            .Select(i => i.ItemCode)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(InventoryItem item)
    {
        _db.InventoryItems.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(InventoryItem item)
    {
        _db.InventoryItems.Update(item);
        await _db.SaveChangesAsync();
    }
}
