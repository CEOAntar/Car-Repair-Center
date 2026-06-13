using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;

namespace CarRepairCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalogService;

    public ServicesController(IServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceDto>>> GetAll([FromQuery] bool? activeOnly)
    {
        var services = await _serviceCatalogService.GetAllAsync(activeOnly);
        return Ok(services);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceDto>> Create(CreateServiceDto dto)
    {
        var result = await _serviceCatalogService.CreateAsync(dto);
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateServiceDto dto)
    {
        var result = await _serviceCatalogService.UpdateAsync(id, dto);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return NoContent();
    }
}
