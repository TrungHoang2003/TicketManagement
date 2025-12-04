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
            var answer = await ragService.GenerateAnswerAsync(request.Query, contextDocuments);

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
}

