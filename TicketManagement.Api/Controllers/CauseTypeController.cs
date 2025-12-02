using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("cause_type")]

public class CauseTypeController(ICauseTypeService causeTypeService) : Controller
{
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateCauseTypeDto createCauseTypeDto)
    {
        var result = await causeTypeService.Create(createCauseTypeDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateCauseTypeDto updateCauseTypeDto)
    {
        var result = await causeTypeService.Update(updateCauseTypeDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int causeTypeId)
    {
        var result = await causeTypeService.Delete(causeTypeId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [HttpGet("get_all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await causeTypeService.GetAll();
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_by_id")]
    public async Task<IActionResult> GetById([FromQuery] int causeTypeId)
    {
        var result = await causeTypeService.GetById(causeTypeId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}
