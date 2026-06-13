using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Core.Enums;

namespace CarRepairCenter.Application.Services;

public class RepairOrderService : IRepairOrderService
{
    private readonly IRepairOrderRepository _repairOrderRepo;
    private readonly IInventoryRepository _inventoryRepo;

    public RepairOrderService(IRepairOrderRepository repairOrderRepo, IInventoryRepository inventoryRepo)
    {
        _repairOrderRepo = repairOrderRepo;
        _inventoryRepo = inventoryRepo;
    }

    public async Task<List<RepairOrderDto>> GetAllAsync(string? status, string? search, int? customerId)
    {
        var orders = await _repairOrderRepo.GetAllAsync(status, search, customerId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<ServiceResult<RepairOrderDto>> GetByIdAsync(int id)
    {
        var r = await _repairOrderRepo.GetByIdAsync(id);
        if (r is null) return ServiceResult<RepairOrderDto>.NotFound();
        return ServiceResult<RepairOrderDto>.Success(MapToDto(r));
    }

    public async Task<List<RepairOrderDto>> GetByVehicleAsync(int vehicleId)
    {
        var orders = await _repairOrderRepo.GetByVehicleAsync(vehicleId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<ServiceResult<RepairOrderDto>> CreateAsync(CreateRepairOrderDto dto, string? userId)
    {
        if (dto.EstimatedCost <= 0)
            return ServiceResult<RepairOrderDto>.Failure("يجب تحديد التكلفة التقديرية أكبر من الصفر");

        var lastCode = await _repairOrderRepo.GetLastOrderCodeAsync();
        var nextNum = 1;
        if (lastCode is not null && int.TryParse(lastCode.Replace("REP-", ""), out var n)) 
            nextNum = n + 1;

        var order = new RepairOrder
        {
            OrderCode = $"REP-{nextNum:D4}",
            CustomerId = dto.CustomerId,
            VehicleId = dto.VehicleId,
            ProblemDescription = dto.ProblemDescription,
            Notes = dto.Notes,
            DiscountPercentage = dto.DiscountPercentage,
            EstimatedCost = dto.EstimatedCost,
            CreatedByUserId = userId
        };

        await _repairOrderRepo.AddAsync(order);

        var loaded = await _repairOrderRepo.GetByIdAsync(order.Id);
        return ServiceResult<RepairOrderDto>.Success(MapToDto(loaded!));
    }

    public async Task<ServiceResult<bool>> UpdateStatusAsync(int id, UpdateRepairOrderStatusDto dto)
    {
        var order = await _repairOrderRepo.GetByIdAsync(id, tracking: true);
        if (order is null) return ServiceResult<bool>.NotFound();
        
        if (!Enum.TryParse<RepairStatus>(dto.Status, true, out var newStatus))
            return ServiceResult<bool>.Failure("حالة غير صالحة");

        // Completed/Delivered orders are FULLY read-only for ALL roles
        if (order.Status == RepairStatus.Done || order.Status == RepairStatus.Delivered)
            return ServiceResult<bool>.Failure("لا يمكن تعديل حالة أمر صيانة مكتمل أو تم تسليمه");

        order.Status = newStatus;
        switch (newStatus)
        {
            case RepairStatus.InProgress: order.StartedAt = DateTime.UtcNow; break;
            case RepairStatus.Done: order.CompletedAt = DateTime.UtcNow; break;
            case RepairStatus.Delivered: order.DeliveredAt = DateTime.UtcNow; break;
        }

        await _repairOrderRepo.UpdateAsync(order);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> UpdateDiscountAsync(int id, decimal discountPercentage)
    {
        var order = await _repairOrderRepo.GetByIdAsync(id, tracking: true);
        if (order is null) return ServiceResult<bool>.NotFound();

        // Completed/Delivered orders are FULLY read-only for ALL roles
        if (order.Status == RepairStatus.Done || order.Status == RepairStatus.Delivered)
            return ServiceResult<bool>.Failure("لا يمكن تعديل هذا الأمر لأنه مكتمل أو تم تسليمه");

        if (order.EstimatedCost > 0)
        {
            var hypotheticalSubTotal = order.SubTotal;
            var hypotheticalDiscount = hypotheticalSubTotal * (discountPercentage / 100m);
            var hypotheticalTotal = hypotheticalSubTotal - hypotheticalDiscount;
            if (hypotheticalTotal > order.EstimatedCost * 2.0m)
            {
                return ServiceResult<bool>.Failure("المبلغ الإجمالي يتجاوز الحد المسموح به للتكلفة التقديرية (الحد الأقصى هو ضعف التكلفة التقديرية)");
            }
        }

        order.DiscountPercentage = discountPercentage;
        await _repairOrderRepo.UpdateAsync(order);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> AddServiceAsync(int id, AddRepairOrderServiceDto dto)
    {
        var order = await _repairOrderRepo.GetByIdAsync(id, tracking: true);
        if (order is null) return ServiceResult<bool>.NotFound();

        // Completed/Delivered orders are FULLY read-only for ALL roles
        if (order.Status == RepairStatus.Done || order.Status == RepairStatus.Delivered)
            return ServiceResult<bool>.Failure("لا يمكن تعديل هذا الأمر لأنه مكتمل أو تم تسليمه");

        if (order.EstimatedCost > 0)
        {
            var currentServicesSum = order.RepairOrderServices.Sum(s => s.Price) + dto.Price;
            var currentPartsSum = order.RepairOrderParts.Sum(p => p.TotalPrice);
            var hypotheticalSubTotal = currentServicesSum + currentPartsSum;
            var hypotheticalDiscount = hypotheticalSubTotal * (order.DiscountPercentage / 100m);
            var hypotheticalTotal = hypotheticalSubTotal - hypotheticalDiscount;
            if (hypotheticalTotal > order.EstimatedCost * 2.0m)
            {
                return ServiceResult<bool>.Failure("المبلغ الإجمالي يتجاوز الحد المسموح به للتكلفة التقديرية (الحد الأقصى هو ضعف التكلفة التقديرية)");
            }
        }

        var service = new CarRepairCenter.Core.Entities.RepairOrderService
        {
            RepairOrderId = id,
            ServiceId = dto.ServiceId,
            Price = dto.Price,
            Notes = dto.Notes
        };

        await _repairOrderRepo.AddServiceAsync(service);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> RemoveServiceAsync(int orderId, int serviceLineId)
    {
        var order = await _repairOrderRepo.GetByIdAsync(orderId, tracking: true);
        if (order is null) return ServiceResult<bool>.NotFound();

        // Completed/Delivered orders are FULLY read-only for ALL roles
        if (order.Status == RepairStatus.Done || order.Status == RepairStatus.Delivered)
            return ServiceResult<bool>.Failure("لا يمكن تعديل هذا الأمر لأنه مكتمل أو تم تسليمه");

        var line = order.RepairOrderServices.FirstOrDefault(s => s.Id == serviceLineId);
        if (line is null) return ServiceResult<bool>.NotFound();

        await _repairOrderRepo.RemoveServiceAsync(line);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<object?>> AddPartAsync(int id, AddRepairOrderPartDto dto)
    {
        var order = await _repairOrderRepo.GetByIdAsync(id, tracking: true);
        if (order is null) return ServiceResult<object?>.NotFound();

        // Completed/Delivered orders are FULLY read-only for ALL roles
        if (order.Status == RepairStatus.Done || order.Status == RepairStatus.Delivered)
            return ServiceResult<object?>.Failure("لا يمكن تعديل هذا الأمر لأنه مكتمل أو تم تسليمه");

        var item = await _inventoryRepo.GetByIdAsync(dto.InventoryItemId);
        if (item is null) return ServiceResult<object?>.Failure("قطعة الغيار غير موجودة");

        var partPrice = dto.UnitPrice ?? item.UnitPrice;
        var partTotalPrice = dto.Quantity * partPrice;

        if (order.EstimatedCost > 0)
        {
            var currentServicesSum = order.RepairOrderServices.Sum(s => s.Price);
            var currentPartsSum = order.RepairOrderParts.Sum(p => p.TotalPrice) + partTotalPrice;
            var hypotheticalSubTotal = currentServicesSum + currentPartsSum;
            var hypotheticalDiscount = hypotheticalSubTotal * (order.DiscountPercentage / 100m);
            var hypotheticalTotal = hypotheticalSubTotal - hypotheticalDiscount;
            if (hypotheticalTotal > order.EstimatedCost * 2.0m)
            {
                return ServiceResult<object?>.Failure("المبلغ الإجمالي يتجاوز الحد المسموح به للتكلفة التقديرية (الحد الأقصى هو ضعف التكلفة التقديرية)");
            }
        }

        // Warn if stock is low but don't block
        var warning = item.Quantity < dto.Quantity
            ? $"تحذير: الكمية المتاحة ({item.Quantity}) أقل من المطلوبة ({dto.Quantity})"
            : null;

        item.Quantity -= dto.Quantity;
        await _inventoryRepo.UpdateAsync(item);

        var part = new RepairOrderPart
        {
            RepairOrderId = id,
            InventoryItemId = dto.InventoryItemId,
            Quantity = dto.Quantity,
            UnitPrice = partPrice
        };

        await _repairOrderRepo.AddPartAsync(part);
        return ServiceResult<object?>.Success(new { warning });
    }

    public async Task<ServiceResult<bool>> RemovePartAsync(int orderId, int partLineId)
    {
        var order = await _repairOrderRepo.GetByIdAsync(orderId, tracking: true);
        if (order is null) return ServiceResult<bool>.NotFound();

        // Completed/Delivered orders are FULLY read-only for ALL roles
        if (order.Status == RepairStatus.Done || order.Status == RepairStatus.Delivered)
            return ServiceResult<bool>.Failure("لا يمكن تعديل هذا الأمر لأنه مكتمل أو تم تسليمه");

        var line = order.RepairOrderParts.FirstOrDefault(p => p.Id == partLineId);
        if (line is null) return ServiceResult<bool>.NotFound();

        // Restore inventory
        var item = await _inventoryRepo.GetByIdAsync(line.InventoryItemId);
        if (item is not null)
        {
            item.Quantity += line.Quantity;
            await _inventoryRepo.UpdateAsync(item);
        }

        await _repairOrderRepo.RemovePartAsync(line);
        return ServiceResult<bool>.Success(true);
    }

    private static RepairOrderDto MapToDto(RepairOrder r) => new(
        r.Id, 
        r.OrderCode,
        r.CustomerId, 
        r.Customer.Name, 
        r.Customer.Phone,
        r.VehicleId, 
        r.Vehicle.PlateNumber, 
        $"{r.Vehicle.Make} {r.Vehicle.Model}",
        r.ProblemDescription, 
        r.Status.ToString(),
        r.DiscountPercentage, 
        r.EstimatedCost,
        r.TotalServicesAmount, 
        r.TotalPartsAmount, 
        r.SubTotal,
        r.DiscountAmount, 
        r.TotalAmount, 
        r.PaidAmount, 
        r.RemainingAmount, 
        r.IsFullyPaid,
        r.Notes, 
        r.CreatedAt, 
        r.StartedAt, 
        r.CompletedAt, 
        r.DeliveredAt,
        r.RepairOrderServices.Select(s => new RepairOrderServiceDto(s.Id, s.ServiceId, s.Service.Name, s.Price, s.Notes)).ToList(),
        r.RepairOrderParts.Select(p => new RepairOrderPartDto(p.Id, p.InventoryItemId, p.InventoryItem.Name, p.Quantity, p.UnitPrice, p.TotalPrice)).ToList(),
        r.Payments.Select(p => new PaymentDto(p.Id, p.RepairOrderId, r.OrderCode, r.Customer.Name, p.Amount, p.PaymentMethod.ToString(), p.Notes, p.PaidAt)).ToList()
    );
}
