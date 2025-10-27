﻿using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("user")]
public class UserController(IUserService userService): Controller
{
    //[Authorize(Policy = "AdminOnly")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest createUserDto)
    {
        var result = await userService.Create(createUserDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
    
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateUserRequest updateUserRequest)
    {
        var result = await userService.Update(updateUserRequest);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [HttpGet("get_by_department")]
    public async Task<IActionResult> GetByDepartment([FromQuery] int departmentId)
    {
        var result = await userService.GetByDepartment(departmentId);
        return Ok(result);
    }
}