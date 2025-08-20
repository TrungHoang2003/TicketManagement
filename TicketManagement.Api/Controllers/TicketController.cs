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
        var userResult = await userService.GetUserId();
        if (!userResult.Success) return BadRequest(userResult.Error);

        dto.CreatorId = userResult.Data;
        
        var result = await ticketService.Create(dto);
        if (result.Success)
            return Ok(result);
        return BadRequest(result.Error);
    }
}