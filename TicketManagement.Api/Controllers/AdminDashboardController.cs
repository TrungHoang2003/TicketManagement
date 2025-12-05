using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("admin/dashboard")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController(IAdminDashboardService adminDashboardService) : ControllerBase
{
    /// <summary>
    /// Get complete admin dashboard data
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await adminDashboardService.GetDashboardAsync();
        if (!result.Success)
            return BadRequest(result.Error);
        
        return Ok(result);
    }

    /// <summary>
    /// Get system overview statistics
    /// </summary>
    [HttpGet("system-overview")]
    public async Task<IActionResult> GetSystemOverview()
    {
        var result = await adminDashboardService.GetSystemOverviewAsync();
        if (!result.Success)
            return BadRequest(result.Error);
        
        return Ok(result);
    }

    /// <summary>
    /// Get employee performance metrics
    /// </summary>
    [HttpGet("employee-performance")]
    public async Task<IActionResult> GetEmployeePerformance(
        [FromQuery] int? departmentId = null, 
        [FromQuery] int topCount = 10)
    {
        var result = await adminDashboardService.GetEmployeePerformanceAsync(departmentId, topCount);
        if (!result.Success)
            return BadRequest(result.Error);
        
        return Ok(result);
    }

    /// <summary>
    /// Get department performance metrics
    /// </summary>
    [HttpGet("department-performance")]
    public async Task<IActionResult> GetDepartmentPerformance()
    {
        var result = await adminDashboardService.GetDepartmentPerformanceAsync();
        if (!result.Success)
            return BadRequest(result.Error);
        
        return Ok(result);
    }

    /// <summary>
    /// Get trend analysis data
    /// </summary>
    [HttpGet("trend-analysis")]
    public async Task<IActionResult> GetTrendAnalysis([FromQuery] int months = 6)
    {
        var result = await adminDashboardService.GetTrendAnalysisAsync(months);
        if (!result.Success)
            return BadRequest(result.Error);
        
        return Ok(result);
    }

    /// <summary>
    /// Get real-time alerts and notifications
    /// </summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetRealtimeAlerts()
    {
        var result = await adminDashboardService.GetRealtimeAlertsAsync();
        if (!result.Success)
            return BadRequest(result.Error);
        
        return Ok(result);
    }

    /// <summary>
    /// Get workload analysis for all users
    /// </summary>
    [HttpGet("workload-analysis")]
    public async Task<IActionResult> GetWorkloadAnalysis()
    {
        var result = await adminDashboardService.GetWorkloadAnalysisAsync();
        if (!result.Success)
            return BadRequest(result.Error);
        
        return Ok(result);
    }

    /// <summary>
    /// Get all users statistics with optional department filter
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsersStatistics([FromQuery] int? departmentId = null)
    {
        var result = await adminDashboardService.GetAllUsersStatisticsAsync(departmentId);
        if (!result.Success)
            return BadRequest(result.Error);
        
        return Ok(result);
    }

    /// <summary>
    /// Get detailed statistics for a specific user
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserDetailStatistics(int userId)
    {
        var result = await adminDashboardService.GetUserDetailStatisticsAsync(userId);
        if (!result.Success)
            return BadRequest(result.Error);
        
        return Ok(result);
    }
}