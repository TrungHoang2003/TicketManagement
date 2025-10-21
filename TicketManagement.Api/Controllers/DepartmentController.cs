using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;


[ApiController]
[Route("department")]
public class DepartmentController(IDepartmentService departmentService): Controller
{
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto createDepartmentDto)
    {
        var result = await departmentService.Create(createDepartmentDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateDepartmentDto updateDepartmentDto)
    {
        var result = await departmentService.Update(updateDepartmentDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int departmentId)
    {
        var result = await departmentService.Delete(departmentId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await departmentService.GetAll();
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_by_id")]
    public async Task<IActionResult> GetById([FromQuery] int departmentId)
    {
        var result = await departmentService.GetById(departmentId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}