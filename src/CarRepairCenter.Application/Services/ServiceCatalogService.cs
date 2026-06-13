using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Services;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _serviceRepo;

    public ServiceCatalogService(IServiceRepository serviceRepo)
    {
        _serviceRepo = serviceRepo;
    }

    public async Task<List<ServiceDto>> GetAllAsync(bool? activeOnly)
    {
        var services = await _serviceRepo.GetAllAsync(activeOnly);
        return services.Select(s => new ServiceDto(
            s.Id, 
            s.Name, 
            s.Description, 
            s.DefaultPrice, 
            s.IsActive
        )).ToList();
    }

    public async Task<ServiceResult<ServiceDto>> CreateAsync(CreateServiceDto dto)
    {
        var service = new Service 
        { 
            Name = dto.Name, 
            Description = dto.Description, 
            DefaultPrice = dto.DefaultPrice 
        };

        await _serviceRepo.AddAsync(service);

        return ServiceResult<ServiceDto>.Success(new ServiceDto(
            service.Id, 
            service.Name, 
            service.Description, 
            service.DefaultPrice, 
            service.IsActive
        ));
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, UpdateServiceDto dto)
    {
        var service = await _serviceRepo.GetByIdAsync(id);
        if (service is null) return ServiceResult<bool>.NotFound();

        service.Name = dto.Name;
        service.Description = dto.Description;
        service.DefaultPrice = dto.DefaultPrice;
        service.IsActive = dto.IsActive;

        await _serviceRepo.UpdateAsync(service);
        return ServiceResult<bool>.Success(true);
    }
}
