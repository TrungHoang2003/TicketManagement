using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class RagController(
    IRagService ragService, 
    IQdrantCacheService qdrantCache,
    ILogger<RagController> logger) : ControllerBase
{
    /// <summary>
    /// Endpoint chính để hỏi đáp RAG với semantic cache
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
            logger.LogInformation("Received RAG query: {Query}", request.Query);

            // Tính embedding của câu hỏi
            var questionEmbedding = await ragService.GetQuestionEmbeddingAsync(request.Query);

            // Tìm trong cache
            var cachedResponse = await qdrantCache.SearchSimilarQuestionAsync(
                request.Query,
                questionEmbedding,
                similarityThreshold: 0.90
            );

            if (cachedResponse != null)
            {
                // Cache HIT
                logger.LogInformation("Cache HIT (similarity: {Score:F3})", cachedResponse.SimilarityScore);

                return Ok(new
                {
                    answer = cachedResponse.Answer,
                    timestamp = DateTime.UtcNow,
                    fromCache = true,
                    cachedMatchQuestion = cachedResponse.OriginalQuestion,
                    similarityScore = cachedResponse.SimilarityScore
                });
            }

            // Cache MISS - chạy RAG đầy đủ
            var contextDocuments = await ragService.RetrieveContextAsync(request.Query, k: 3);

            var answerStream = ragService.GenerateAnswerStreamAsync(request.Query, contextDocuments);
            var answerTokens = new List<string>();
            await foreach (var token in answerStream)
            {
                answerTokens.Add(token);
            }
            var answer = string.Concat(answerTokens);

            // Cache kết quả
            _ = Task.Run(async () =>
            {
                try
                {
                    await qdrantCache.CacheResponseAsync(request.Query, questionEmbedding, answer);
                }
                catch (Exception cacheEx)
                {
                    logger.LogWarning(cacheEx, "Failed to cache response");
                }
            });

            return Ok(new
            {
                answer,
                timestamp = DateTime.UtcNow,
                fromCache = false
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing RAG query: {Query}", request.Query);
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Endpoint stream để nhận câu trả lời theo thời gian thực (SSE)
    /// </summary>
    [HttpPost("ask-stream")]
    public async Task AskStream([FromBody] RagQueryDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            Response.StatusCode = 400;
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        try
        {
            logger.LogInformation("Received RAG stream query: {Query}", request.Query);

            // Retrieve context
            var contextDocuments = await ragService.RetrieveContextAsync(request.Query, k: 2);

            // Stream answer
            var answerStream = ragService.GenerateAnswerStreamAsync(request.Query, contextDocuments);
            
            await foreach (var token in answerStream)
            {
                var data = System.Text.Json.JsonSerializer.Serialize(new { token });
                await Response.WriteAsync($"data: {data}\n\n");
                await Response.Body.FlushAsync();
            }

            // Send completion signal
            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing RAG stream query: {Query}", request.Query);
            var errorData = System.Text.Json.JsonSerializer.Serialize(new { error = ex.Message });
            await Response.WriteAsync($"data: {errorData}\n\n");
            await Response.Body.FlushAsync();
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
            var contextDocuments = await ragService.RetrieveContextAsync(request.Query, k);
            
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
            logger.LogError(ex, "Error retrieving context for query: {Query}", request.Query);
            return StatusCode(500, new { error = "An error occurred while retrieving context" });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "RAG", timestamp = DateTime.UtcNow });
    }
}

