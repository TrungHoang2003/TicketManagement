using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RagController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly ILogger<RagController> _logger;

    public RagController(IRagService ragService, ILogger<RagController> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint chính để hỏi đáp RAG
    /// Nhận câu hỏi từ người dùng, tìm context liên quan và sinh câu trả lời
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] RagQueryDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(new { error = "Query cannot be empty" });
        }

        try
        {
            _logger.LogInformation("Received RAG query: {Query}", request.Query);

            // Bước 1: Retrieve - Tìm context liên quan
            var contextDocuments = await _ragService.RetrieveContextAsync(request.Query, k: 5);

            // Bước 2: Generate - Sinh câu trả lời (collect stream thành string)
            var answerStream = _ragService.GenerateAnswerStreamAsync(request.Query, contextDocuments);
            var answerTokens = new List<string>();
            await foreach (var token in answerStream)
            {
                answerTokens.Add(token);
            }
            var answer = string.Concat(answerTokens);

            var response = new RagResponseDto(
                Answer: answer,
                SourceDocuments: contextDocuments,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RAG query: {Query}", request.Query);
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Endpoint chỉ để retrieve context mà không generate
    /// Hữu ích cho debugging hoặc khi muốn xem context trước khi sinh câu trả lời
    /// </summary>
    [HttpPost("retrieve")]
    public async Task<IActionResult> Retrieve([FromBody] RagQueryDto request, [FromQuery] int k = 5)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(new { error = "Query cannot be empty" });
        }

        try
        {
            var contextDocuments = await _ragService.RetrieveContextAsync(request.Query, k);
            
            return Ok(new
            {
                query = request.Query,
                documents = contextDocuments,
                count = contextDocuments.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving context for query: {Query}", request.Query);
            return StatusCode(500, new { error = "An error occurred while retrieving context" });
        }
    }

    /// <summary>
    /// Health check endpoint để kiểm tra service có hoạt động không
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "RAG", timestamp = DateTime.UtcNow });
    }
}
