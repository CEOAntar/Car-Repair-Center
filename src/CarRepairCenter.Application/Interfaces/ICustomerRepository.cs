using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync(string? search);
    Task<Customer?> GetByIdAsync(int id);
    Task<string?> GetLastCustomerCodeAsync();
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
}
