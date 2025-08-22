using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("user")]
public class UserController(IUserService userService): Controller
{
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
    {
        var result = await userService.Create(createUserDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
    
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto updateUserDto)
    {
        var result = await userService.Update(updateUserDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}