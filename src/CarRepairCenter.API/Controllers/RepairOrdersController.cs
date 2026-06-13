using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;

namespace CarRepairCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepairOrdersController : ControllerBase
{
    private readonly IRepairOrderService _repairOrderService;

    public RepairOrdersController(IRepairOrderService repairOrderService)
    {
        _repairOrderService = repairOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RepairOrderDto>>> GetAll(
        [FromQuery] string? status, 
        [FromQuery] string? search, 
        [FromQuery] int? customerId)
    {
        var orders = await _repairOrderService.GetAllAsync(status, search, customerId);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RepairOrderDto>> GetById(int id)
    {
        var result = await _repairOrderService.GetByIdAsync(id);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("vehicle/{vehicleId}")]
    public async Task<ActionResult<List<RepairOrderDto>>> GetByVehicle(int vehicleId)
    {
        var orders = await _repairOrderService.GetByVehicleAsync(vehicleId);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult<RepairOrderDto>> Create(CreateRepairOrderDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _repairOrderService.CreateAsync(dto, userId);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateRepairOrderStatusDto dto)
    {
        var result = await _repairOrderService.UpdateStatusAsync(id, dto);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }

    [HttpPatch("{id}/discount")]
    public async Task<IActionResult> UpdateDiscount(int id, [FromBody] decimal discountPercentage)
    {
        var result = await _repairOrderService.UpdateDiscountAsync(id, discountPercentage);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }

    // ── Services on Order ──
    [HttpPost("{id}/services")]
    public async Task<IActionResult> AddService(int id, AddRepairOrderServiceDto dto)
    {
        var result = await _repairOrderService.AddServiceAsync(id, dto);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok();
    }

    [HttpDelete("{orderId}/services/{serviceLineId}")]
    public async Task<IActionResult> RemoveService(int orderId, int serviceLineId)
    {
        var result = await _repairOrderService.RemoveServiceAsync(orderId, serviceLineId);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }

    // ── Parts on Order ──
    [HttpPost("{id}/parts")]
    public async Task<IActionResult> AddPart(int id, AddRepairOrderPartDto dto)
    {
        var result = await _repairOrderService.AddPartAsync(id, dto);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpDelete("{orderId}/parts/{partLineId}")]
    public async Task<IActionResult> RemovePart(int orderId, int partLineId)
    {
        var result = await _repairOrderService.RemovePartAsync(orderId, partLineId);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }
}
