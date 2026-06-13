using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Interfaces;

public interface IServiceRepository
{
    Task<List<Service>> GetAllAsync(bool? activeOnly);
    Task<Service?> GetByIdAsync(int id);
    Task AddAsync(Service service);
    Task UpdateAsync(Service service);
}
