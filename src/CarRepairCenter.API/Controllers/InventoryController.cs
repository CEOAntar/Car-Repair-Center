using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;

namespace CarRepairCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<InventoryItemDto>>> GetAll([FromQuery] string? search)
    {
        var items = await _inventoryService.GetAllAsync(search);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<InventoryItemDto>> Create(CreateInventoryItemDto dto)
    {
        var result = await _inventoryService.CreateAsync(dto);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateInventoryItemDto dto)
    {
        var result = await _inventoryService.UpdateAsync(id, dto);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }
}
