using CarRepairCenter.Application.Common;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Core.Enums;

namespace CarRepairCenter.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IRepairOrderRepository _repairOrderRepo;

    public PaymentService(IPaymentRepository paymentRepo, IRepairOrderRepository repairOrderRepo)
    {
        _paymentRepo = paymentRepo;
        _repairOrderRepo = repairOrderRepo;
    }

    public async Task<List<PaymentDto>> GetAllAsync(int? repairOrderId, DateTime? date, int? customerId, string? search)
    {
        var payments = await _paymentRepo.GetAllAsync(repairOrderId, date, customerId, search);
        return payments.Select(p => new PaymentDto(
            p.Id, 
            p.RepairOrderId, 
            p.RepairOrder.OrderCode, 
            p.RepairOrder.Customer.Name, 
            p.Amount, 
            p.PaymentMethod.ToString(), 
            p.Notes, 
            p.PaidAt
        )).ToList();
    }

    public async Task<ServiceResult<PaymentDto>> CreateAsync(CreatePaymentDto dto, string? userId)
    {
        var order = await _repairOrderRepo.GetByIdAsync(dto.RepairOrderId, tracking: true);
        if (order is null) return ServiceResult<PaymentDto>.Failure("أمر الصيانة غير موجود");

        if (dto.Amount <= 0)
            return ServiceResult<PaymentDto>.Failure("يجب أن تكون قيمة الدفعة أكبر من الصفر");

        if (dto.Amount > order.RemainingAmount)
            return ServiceResult<PaymentDto>.Failure("قيمة الدفعة تتجاوز المبلغ المتبقي المطلوب للمطالبة!");

        if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var method))
            return ServiceResult<PaymentDto>.Failure("طريقة دفع غير صالحة");

        var payment = new Payment
        {
            RepairOrderId = dto.RepairOrderId,
            Amount = dto.Amount,
            PaymentMethod = method,
            Notes = dto.Notes,
            RecordedByUserId = userId
        };

        await _paymentRepo.AddAsync(payment);

        return ServiceResult<PaymentDto>.Success(new PaymentDto(
            payment.Id, 
            payment.RepairOrderId, 
            order.OrderCode, 
            order.Customer.Name, 
            payment.Amount, 
            payment.PaymentMethod.ToString(), 
            payment.Notes, 
            payment.PaidAt
        ));
    }
}
