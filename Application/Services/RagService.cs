using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Settings;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class RagService : IRagService
{
    private readonly IEmbeddingRepository _embeddingRepository;
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _ollamaSettings;
    private readonly RagPromptSettings _ragPromptSettings;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IEmbeddingRepository embeddingRepository,
        HttpClient httpClient,
        IOptions<OllamaSettings> ollamaSettings,
        IOptions<RagPromptSettings> ragPromptSettings,
        ILogger<RagService> logger)
    {
        _embeddingRepository = embeddingRepository;
        _httpClient = httpClient;
        _ollamaSettings = ollamaSettings.Value;
        _ragPromptSettings = ragPromptSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Bước 1: Retrieval - Tìm các document liên quan từ vector database
    /// </summary>
    public async Task<List<string>> RetrieveContextAsync(string query, int k = 3)
    {
        try
        {
            _logger.LogInformation("Retrieving context for query: {Query}", query);
            
            // 1. Tạo embedding cho câu query
            var queryEmbedding = await GetEmbeddingAsync(query);
            
            // 2. Tìm kiếm các document tương tự trong database
            var documents = await _embeddingRepository.SearchSimilarDocumentsAsync(queryEmbedding, k);
            
            _logger.LogInformation("Retrieved {Count} documents for context", documents.Count);
            
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving context for query: {Query}", query);
            throw;
        }
    }

    /// <summary>
    /// Bước 2: Generation - Sinh câu trả lời dựa trên context đã retrieve
    /// </summary>
    public async Task<string> GenerateAnswerAsync(string userQuery, List<string> context)
    {
        try
        {
            _logger.LogInformation("Generating answer for query: {Query}", userQuery);
            
            // Tạo prompt với context
            var contextText = string.Join("\n\n---\n\n", context);
            var prompt = $"""
                {_ragPromptSettings.SystemInstruction}

                ### NGỮ CẢNH TỪ TÀI LIỆU:
                {contextText}

                ### CÂU HỎI CỦA NGƯỜI DÙNG:
                {userQuery}

                ### TRẢ LỜI:
                """;

            // Gọi Ollama API để generate
            var answer = await GenerateWithOllamaAsync(prompt);
            
            _logger.LogInformation("Generated answer successfully");
            
            return answer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating answer for query: {Query}", userQuery);
            throw;
        }
    }

    /// <summary>
    /// Tạo embedding vector cho text sử dụng Ollama
    /// </summary>
    private async Task<float[]> GetEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            model = _ollamaSettings.EmbeddingModel,
            input = text
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_ollamaSettings.BaseUrl}/api/embed", 
            requestBody);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>();
        
        if (result?.Embeddings == null || result.Embeddings.Length == 0)
        {
            throw new InvalidOperationException("Failed to get embedding from Ollama");
        }
        
        return result.Embeddings[0];
    }

    /// <summary>
    /// Gọi Ollama API để generate text
    /// </summary>
    private async Task<string> GenerateWithOllamaAsync(string prompt)
    {
        var requestBody = new
        {
            model = _ollamaSettings.GenerationModel,
            prompt = prompt,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_ollamaSettings.BaseUrl}/api/generate", 
            requestBody);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
        
        return result?.Response ?? string.Empty;
    }
}

// DTOs cho Ollama API responses
internal class OllamaEmbedResponse
{
    public float[][] Embeddings { get; set; } = Array.Empty<float[]>();
}

internal class OllamaGenerateResponse
{
    public string Response { get; set; } = string.Empty;
    public bool Done { get; set; }
}
