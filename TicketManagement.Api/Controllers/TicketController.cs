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
    public async Task<IActionResult> CreateTicket(CreateTicketRequest request)
    {
        var result = await ticketService.Create(request);
        if (result.Success)
            return Ok(result);
        return BadRequest(result.Error);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign(AssignTicketRequest ticketRequest)
    {
        var result = await ticketService.Assign(ticketRequest);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
    
    [HttpPost("add-head")]
    public async Task<IActionResult> AddHead(AddHeadRequest request)
    {
        var result = await ticketService.AddHead(request);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }

    [HttpPost("handle")]
    public async Task<IActionResult> Handle([FromQuery] int ticketId)
    {
        var result = await ticketService.HandleTicket(ticketId);
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
    
    [HttpPost("get_list")]
    public async Task<IActionResult> GetList(GetListTicketRequest request)
    {
        var result = await ticketService.GetListTicket(request);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
    
    [HttpPost("get_detail")]
    public async Task<IActionResult> GetDetail([FromQuery] int ticketId)
    {
        var result = await ticketService.GetDetailTicket(ticketId);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
}