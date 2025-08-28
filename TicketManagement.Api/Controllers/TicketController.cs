using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TicketController(IUserService userService, ITicketService ticketService): Controller
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateTicket(CreateTicketDto dto)
    {
        var userResult = await userService.GetLoginUserId();
        if (!userResult.Success) return BadRequest(userResult.Error);

        dto.CreatorId = userResult.Data;
        
        var result = await ticketService.Create(dto);
        if (result.Success)
            return Ok(result);
        return BadRequest(result.Error);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(AssignDto dto)
    {
        var userResult = await userService.GetLoginUserId();
        if (!userResult.Success) return BadRequest(userResult.Error);
        
        var result = await ticketService.Assign(dto, userResult.Data);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [HttpPost("reject")]
    public async Task<IActionResult> Reject(RejectTicketDto dto)
    {
        var result = await ticketService.RejectTicket(dto);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}