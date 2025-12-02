using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("role")]
public class RoleController(IRoleService roleService) : Controller
{
    // [Authorize(Policy = "AdminOnly")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto createRoleDto)
    {
        var result = await roleService.Create(createRoleDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    // [Authorize(Policy = "AdminOnly")]
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateRoleDto updateRoleDto)
    {
        var result = await roleService.Update(updateRoleDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    // [Authorize(Policy = "AdminOnly")]
    [HttpDelete("delete/{roleId}")]
    public async Task<IActionResult> Delete(int roleId)
    {
        var result = await roleService.Delete(roleId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    // [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await roleService.GetAll();
        return Ok(result);
    }

    // [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_by_id/{roleId}")]
    public async Task<IActionResult> GetById(int roleId)
    {
        var result = await roleService.GetById(roleId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}
