using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("category")]
public class CategoryController(ICategoryService categoryService) : Controller
{
    // [Authorize(Policy = "AdminOnly")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
    {
        var result = await categoryService.Create(createCategoryDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    // [Authorize(Policy = "AdminOnly")]
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateCategoryDto updateCategoryDto)
    {
        var result = await categoryService.Update(updateCategoryDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    // [Authorize(Policy = "AdminOnly")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int categoryId)
    {
        var result = await categoryService.Delete(categoryId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    // [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await categoryService.GetAll();
        return Ok(result);
    }

    // [Authorize(Policy = "AdminOnly")]
    [HttpGet("get_by_id")]
    public async Task<IActionResult> GetById([FromQuery] int categoryId)
    {
        var result = await categoryService.GetById(categoryId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}