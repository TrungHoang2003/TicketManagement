﻿using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("auth")]

public class AuthenticationController(IUserService userService, IGoogleAuthService googleAuthService): Controller
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        var result = await userService.Login(userLoginDto);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [HttpGet("google-login")]
    public Task<IActionResult> GoogleLogin()
    {
        var url = googleAuthService.GetGoogleAuthUrl();
        return Task.FromResult<IActionResult>(Ok(url));
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallBack([FromQuery] string code)
    {
        var result = await googleAuthService.GoogleCallBack(code);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}