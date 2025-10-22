using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;
[ApiController]
[Route("implementation_plan")]
public class ImplementationPlanController(IImplementationPlanService implementationPlanService) : Controller
{
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateImplementationPlanDto createImplementationPlanDto)
    {
        var result = await implementationPlanService.Create(createImplementationPlanDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateImplementationPlanDto updateImplementationPlanDto)
    {
        var result = await implementationPlanService.Update(updateImplementationPlanDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int implementationPlanId)
    {
        var result = await implementationPlanService.Delete(implementationPlanId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await implementationPlanService.GetAll();
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_by_id")]
    public async Task<IActionResult> GetById([FromQuery] int implementationPlanId)
    {
        var result = await implementationPlanService.GetById(implementationPlanId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}
