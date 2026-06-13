using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;

namespace CarRepairCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VehicleDto>>> GetAll([FromQuery] int? customerId, [FromQuery] string? search)
    {
        var vehicles = await _vehicleService.GetAllAsync(customerId, search);
        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleDto>> GetById(int id)
    {
        var result = await _vehicleService.GetByIdAsync(id);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> Create(CreateVehicleDto dto)
    {
        var result = await _vehicleService.CreateAsync(dto);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateVehicleDto dto)
    {
        var result = await _vehicleService.UpdateAsync(id, dto);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult> GetHistory(int id)
    {
        var result = await _vehicleService.GetHistoryAsync(id);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }
}
