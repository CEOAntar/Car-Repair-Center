using Microsoft.EntityFrameworkCore;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Infrastructure.Data;

namespace CarRepairCenter.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Customer>> GetAllAsync(string? search)
    {
        var query = _db.Customers.Include(c => c.Vehicles).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search) 
                || c.Phone.Contains(search) 
                || c.CustomerCode.Contains(search));
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _db.Customers.Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<string?> GetLastCustomerCodeAsync()
    {
        return await _db.Customers
            .OrderByDescending(c => c.Id)
            .Select(c => c.CustomerCode)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Customer customer)
    {
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
    }
}
