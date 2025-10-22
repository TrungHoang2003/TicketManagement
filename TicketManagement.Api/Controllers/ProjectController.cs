using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;
[ApiController]
[Route("project")]
public class ProjectController(IProjectService projectService) : Controller
{
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto createProjectDto)
    {
        var result = await projectService.Create(createProjectDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateProjectDto updateProjectDto)
    {
        var result = await projectService.Update(updateProjectDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int projectId)
    {
        var result = await projectService.Delete(projectId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await projectService.GetAll();
        return Ok(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_by_id")]
    public async Task<IActionResult> GetById([FromQuery] int projectId)
    {
        var result = await projectService.GetById(projectId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}