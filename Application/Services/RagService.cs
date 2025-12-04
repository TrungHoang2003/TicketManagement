using BuildingBlocks.Settings;
using Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using OllamaSharp; // Sử dụng thư viện này
using OllamaSharp.Models; // Để dùng các object Request/Response chuẩn

namespace Application.Services;

public class RagService(
    IEmbeddingRepository embeddingRepository,
    IOllamaApiClient ollamaClient, // Inject thư viện vào
    IOptions<OllamaSettings> ollamaSettings,
    IOptions<RagPromptSettings> ragPromptSettings)
    : IRagService
{
    private readonly OllamaSettings _ollamaSettings = ollamaSettings.Value;
    private readonly RagPromptSettings _ragPromptSettings = ragPromptSettings.Value;

    public async Task<List<string>> RetrieveContextAsync(string query, int k)
    {
        ollamaClient.SelectedModel = _ollamaSettings.EmbeddingModel;     
        var queryResponse = await ollamaClient.EmbedAsync(query); 
        var embedResult = queryResponse.Embeddings.First().ToArray();
        
        return await embeddingRepository.SearchSimilarDocumentsAsync(embedResult, k);
    }

    public async Task<string> GenerateAnswerAsync(string userQuery, List<string> context)
    {
        var contextText = string.Join("\n\n", context);
        var prompt = $"{_ragPromptSettings.SystemInstruction}\n\nNgữ cảnh:\n{contextText}\n\nCâu hỏi: {userQuery}\n\nTrả lời ngắn gọn:";

        ollamaClient.SelectedModel = _ollamaSettings.GenerationModel;

        var request = new GenerateRequest
        {
            Prompt = prompt,
            Options = new RequestOptions
            {
                Temperature = 0.3f,
                NumPredict = 1024
            }
        };
        
        // Collect all tokens from stream
        var answerBuilder = new System.Text.StringBuilder();
        await foreach (var stream in ollamaClient.GenerateAsync(request))
        {
            answerBuilder.Append(stream.Response);
        }
        
        return answerBuilder.ToString();
    }

    /// <summary>
    /// Tính embedding (vector) của câu hỏi
    /// Dùng để so sánh semantic similarity trong cache
    /// </summary>
    public async Task<float[]> GetQuestionEmbeddingAsync(string question)
    {
        // Chọn model embedding
        ollamaClient.SelectedModel = _ollamaSettings.EmbeddingModel;
        
        // Gọi Ollama API để tính embedding
        var response = await ollamaClient.EmbedAsync(question);
        
        // Lấy vector embedding
        var embedding = response.Embeddings.First().ToArray();
        
        return embedding;
    }
}