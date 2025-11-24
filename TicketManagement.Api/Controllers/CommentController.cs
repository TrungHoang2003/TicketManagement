using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CommentController(ICommentService commentService): Controller
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        var result = await commentService.CreateComment(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetTicketComments([FromQuery] int ticketId)
    {
        var result =  await commentService.GetTicketComments(ticketId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}