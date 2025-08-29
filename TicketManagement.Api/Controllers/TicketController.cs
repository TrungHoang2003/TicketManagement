using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TicketController(ITicketService ticketService): Controller
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateTicket(CreateTicketDto dto)
    {
        var result = await ticketService.Create(dto);
        if (result.Success)
            return Ok(result);
        return BadRequest(result.Error);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(AssignDto dto)
    {
        var result = await ticketService.Assign(dto);
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