using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProgressController(IProgressService progressService): Controller
{
    [HttpGet("get_ticket_progresses")]
   public async Task<IActionResult> GetTicketProgresses([FromQuery] int ticketId)
   {
       var result = await progressService.GetTicketProgresses(ticketId); 
       return result.Success ? Ok(result) : BadRequest(result);
   }
}