using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRepairCenter.Application.DTOs;
using CarRepairCenter.Application.Interfaces;

namespace CarRepairCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return Ok(dashboard);
    }

    [HttpGet("report")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DailyReportDto>> DailyReport([FromQuery] DateTime? date)
    {
        var result = await _dashboardService.GetDailyReportAsync(date);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return BadRequest(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }
}
