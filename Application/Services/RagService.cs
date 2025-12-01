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
    private readonly IOllamaApiClient _ollamaClient = ollamaClient;
    private readonly OllamaSettings _ollamaSettings = ollamaSettings.Value;
    private readonly RagPromptSettings _ragPromptSettings = ragPromptSettings.Value;

    public async Task<List<string>> RetrieveContextAsync(string query, int k = 3)
    {
        var queryResponse = await _ollamaClient.EmbedAsync(query);
        var embedResult = queryResponse.Embeddings.First().ToArray();
        
        return await embeddingRepository.SearchSimilarDocumentsAsync(embedResult, k);
    }

    public async IAsyncEnumerable<string> GenerateAnswerStreamAsync(string userQuery, List<string> context)
    {
        var contextText = string.Join("\n\n---\n\n", context);
        var prompt = $"{_ragPromptSettings.SystemInstruction}\n\nCONTEXT:\n{contextText}\n\nQUESTION:\n{userQuery}";

        _ollamaClient.SelectedModel = _ollamaSettings.GenerationModel;

        // Stream dữ liệu trực tiếp về cho Controller/Client
        await foreach (var stream in _ollamaClient.GenerateAsync(prompt))
        {
            yield return stream.Response;
        }
    }
}