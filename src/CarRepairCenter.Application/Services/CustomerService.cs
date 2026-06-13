using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;

namespace CarRepairCenter.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepo;

    public CustomerService(ICustomerRepository customerRepo)
    {
        _customerRepo = customerRepo;
    }

    public async Task<List<CustomerDto>> GetAllAsync(string? search)
    {
        var customers = await _customerRepo.GetAllAsync(search);
        return customers.Select(c => new CustomerDto(
            c.Id, 
            c.CustomerCode, 
            c.Name, 
            c.Phone, 
            c.Phone2, 
            c.Address, 
            c.Notes, 
            c.CreatedAt, 
            c.Vehicles.Count
        )).ToList();
    }

    public async Task<ServiceResult<CustomerDto>> GetByIdAsync(int id)
    {
        var c = await _customerRepo.GetByIdAsync(id);
        if (c is null) return ServiceResult<CustomerDto>.NotFound();

        return ServiceResult<CustomerDto>.Success(new CustomerDto(
            c.Id, 
            c.CustomerCode, 
            c.Name, 
            c.Phone, 
            c.Phone2, 
            c.Address, 
            c.Notes, 
            c.CreatedAt, 
            c.Vehicles.Count
        ));
    }

    public async Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerDto dto)
    {
        // Auto-generate code
        var lastCode = await _customerRepo.GetLastCustomerCodeAsync();
        var nextNum = 1;
        if (lastCode is not null && int.TryParse(lastCode.Replace("CUS-", ""), out var n)) 
            nextNum = n + 1;

        var customer = new Customer
        {
            CustomerCode = $"CUS-{nextNum:D4}",
            Name = dto.Name,
            Phone = dto.Phone,
            Phone2 = dto.Phone2,
            Address = dto.Address,
            Notes = dto.Notes
        };

        await _customerRepo.AddAsync(customer);

        return ServiceResult<CustomerDto>.Success(new CustomerDto(
            customer.Id, 
            customer.CustomerCode, 
            customer.Name, 
            customer.Phone, 
            customer.Phone2, 
            customer.Address, 
            customer.Notes, 
            customer.CreatedAt, 
            0
        ));
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _customerRepo.GetByIdAsync(id);
        if (customer is null) return ServiceResult<bool>.NotFound();

        customer.Name = dto.Name;
        customer.Phone = dto.Phone;
        customer.Phone2 = dto.Phone2;
        customer.Address = dto.Address;
        customer.Notes = dto.Notes;

        await _customerRepo.UpdateAsync(customer);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var customer = await _customerRepo.GetByIdAsync(id);
        if (customer is null) return ServiceResult<bool>.NotFound();

        await _customerRepo.DeleteAsync(customer);
        return ServiceResult<bool>.Success(true);
    }
}
