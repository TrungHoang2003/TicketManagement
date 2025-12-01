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
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var result = await ticketService.Create(request);
        if (result.Success)
            return Ok(result);
        return BadRequest(result.Error);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignTicketRequest ticketRequest)
    {
        var result = await ticketService.Assign(ticketRequest);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
    
    [HttpPost("unassign")]
    public async Task<IActionResult> Unassign([FromBody] UnassignEmployeeRequest request)
    {
        var result = await ticketService.UnassignEmployee(request);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
    
    [HttpPost("add-head")]
    public async Task<IActionResult> AddHead([FromBody] AddHeadRequest request)
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
    public async Task<IActionResult> Reject([FromBody] RejectTicketDto dto)
    {
        var result = await ticketService.RejectTicket(dto);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
    
    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteTicketDto dto)
    {
        var result = await ticketService.CompleteTicket(dto);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
    
    [HttpPut("update")]
    public async Task<IActionResult> UpdateTicket([FromBody] UpdateTicketRequest request)
    {
        var result = await ticketService.Update(request);
        if(!result.Success) return BadRequest(result.Error);
        return Ok(result);
    }
    
    [HttpPost("get_list")]
    public async Task<IActionResult> GetList([FromBody] GetListTicketRequest request)
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