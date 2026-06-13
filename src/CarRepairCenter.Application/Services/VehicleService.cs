using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly ICustomerRepository _customerRepo;

    public VehicleService(IVehicleRepository vehicleRepo, ICustomerRepository customerRepo)
    {
        _vehicleRepo = vehicleRepo;
        _customerRepo = customerRepo;
    }

    public async Task<List<VehicleDto>> GetAllAsync(int? customerId, string? search)
    {
        var vehicles = await _vehicleRepo.GetAllAsync(customerId, search);
        return vehicles.Select(v => new VehicleDto(
            v.Id, 
            v.CustomerId, 
            v.Customer.Name, 
            v.PlateNumber, 
            v.Make, 
            v.Model, 
            v.Year, 
            v.Color, 
            v.VIN, 
            v.Notes, 
            v.CreatedAt
        )).ToList();
    }

    public async Task<ServiceResult<VehicleDto>> GetByIdAsync(int id)
    {
        var v = await _vehicleRepo.GetByIdAsync(id);
        if (v is null) return ServiceResult<VehicleDto>.NotFound();

        return ServiceResult<VehicleDto>.Success(new VehicleDto(
            v.Id, 
            v.CustomerId, 
            v.Customer.Name, 
            v.PlateNumber, 
            v.Make, 
            v.Model, 
            v.Year, 
            v.Color, 
            v.VIN, 
            v.Notes, 
            v.CreatedAt
        ));
    }

    public async Task<ServiceResult<VehicleDto>> CreateAsync(CreateVehicleDto dto)
    {
        var customer = await _customerRepo.GetByIdAsync(dto.CustomerId);
        if (customer is null) return ServiceResult<VehicleDto>.Failure("العميل غير موجود");

        var vehicle = new Vehicle
        {
            CustomerId = dto.CustomerId,
            PlateNumber = dto.PlateNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            Color = dto.Color,
            VIN = dto.VIN,
            Notes = dto.Notes
        };

        await _vehicleRepo.AddAsync(vehicle);

        return ServiceResult<VehicleDto>.Success(new VehicleDto(
            vehicle.Id, 
            vehicle.CustomerId, 
            customer.Name, 
            vehicle.PlateNumber, 
            vehicle.Make, 
            vehicle.Model, 
            vehicle.Year, 
            vehicle.Color, 
            vehicle.VIN, 
            vehicle.Notes, 
            vehicle.CreatedAt
        ));
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, CreateVehicleDto dto)
    {
        var vehicle = await _vehicleRepo.GetByIdAsync(id);
        if (vehicle is null) return ServiceResult<bool>.NotFound();

        vehicle.PlateNumber = dto.PlateNumber;
        vehicle.Make = dto.Make;
        vehicle.Model = dto.Model;
        vehicle.Year = dto.Year;
        vehicle.Color = dto.Color;
        vehicle.VIN = dto.VIN;
        vehicle.Notes = dto.Notes;

        await _vehicleRepo.UpdateAsync(vehicle);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<object>> GetHistoryAsync(int id)
    {
        var vehicle = await _vehicleRepo.GetByIdAsync(id);
        if (vehicle is null) return ServiceResult<object>.NotFound();

        var orders = await _vehicleRepo.GetHistoryByVehicleIdAsync(id);
        var result = orders.Select(r => new
        {
            r.Id,
            r.OrderCode,
            TotalAmount = (
                (r.RepairOrderServices.Sum(s => (decimal?)s.Price) ?? 0m)
                + (r.RepairOrderParts.Sum(p => (decimal?)(p.Quantity * p.UnitPrice)) ?? 0m)
            ) * (1 - r.DiscountPercentage / 100m),
            Status = r.Status.ToString(),
            r.CreatedAt
        }).ToList<object>();

        return ServiceResult<object>.Success(result);
    }
}
