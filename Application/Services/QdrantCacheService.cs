using BuildingBlocks.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Application.Services;

/// <summary>
/// Service để cache RAG responses sử dụng Qdrant vector database
/// </summary>
public interface IQdrantCacheService
{
    Task InitializeCollectionAsync();
    Task CacheResponseAsync(string question, float[] questionEmbedding, string answer);
    Task<CachedRagResponse?> SearchSimilarQuestionAsync(string question, float[] questionEmbedding, double similarityThreshold = 0.90);
}

public class CachedRagResponse
{
    public string OriginalQuestion { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public int HitCount { get; set; }
}

public class QdrantCacheService : IQdrantCacheService
{
    private readonly QdrantClient _qdrantClient;
    private readonly ILogger<QdrantCacheService> _logger;
    private readonly QdrantSettings _qdrantSettings;

    public QdrantCacheService(
        ILogger<QdrantCacheService> logger, IOptions<QdrantSettings> qdrantSettings)
    {
        _logger = logger;
        _qdrantSettings = qdrantSettings.Value;

        var qdrantHost = _qdrantSettings.Host;
        var qdrantPort = _qdrantSettings.Port;

        _qdrantClient = new QdrantClient(
            host: qdrantHost,
            port: qdrantPort,
            https: false
        );

        _logger.LogInformation("Qdrant client initialized: {Host}:{Port}", qdrantHost, qdrantPort);
    }

    public async Task InitializeCollectionAsync()
    {
        try
        {
            var collections = await _qdrantClient.ListCollectionsAsync();
            var exists = collections?.Contains(_qdrantSettings.CollectionName) ?? false;

            if (!exists)
            {
                await _qdrantClient.CreateCollectionAsync(
                    collectionName: _qdrantSettings.CollectionName,
                    vectorsConfig: new VectorParams
                    {
                        Size = _qdrantSettings.VectorSize,
                        Distance = Distance.Cosine
                    }
                );

                await _qdrantClient.CreatePayloadIndexAsync(
                    collectionName: _qdrantSettings.CollectionName,
                    fieldName: "hit_count",
                    schemaType: PayloadSchemaType.Integer
                );

                _logger.LogInformation("✅ Created Qdrant collection: {Collection}", _qdrantSettings.CollectionName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Qdrant collection");
            throw;
        }
    }

    public async Task CacheResponseAsync(
        string question,
        float[] questionEmbedding,
        string answer)
    {
        try
        {
            var pointId = Guid.NewGuid();

            var payload = new Dictionary<string, Value>
            {
                ["question"] = question,
                ["answer"] = answer,
                ["timestamp"] = DateTime.UtcNow.ToString("o"),
                ["hit_count"] = 0
            };

            var point = new PointStruct
            {
                Id = pointId,
                Vectors = questionEmbedding,
                Payload = { payload }
            };

            await _qdrantClient.UpsertAsync(
                collectionName: _qdrantSettings.CollectionName,
                points: [point]
            );

            _logger.LogDebug("Cached question: {Question}", question);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache response");
        }
    }

    public async Task<CachedRagResponse?> SearchSimilarQuestionAsync(
        string question,
        float[] questionEmbedding,
        double similarityThreshold = 0.90)
    {
        try
        {
            var searchResult = await _qdrantClient.SearchAsync(
                collectionName: _qdrantSettings.CollectionName,
                vector: questionEmbedding,
                limit: 1,
                scoreThreshold: (float)similarityThreshold,
                payloadSelector: true
            );

            var bestMatch = searchResult.FirstOrDefault();
            if (bestMatch == null)
            {
                return null;
            }

            var payload = bestMatch.Payload;
            var cachedQuestion = payload["question"].StringValue;
            var answer = payload["answer"].StringValue;
            var hitCount = (int)payload["hit_count"].IntegerValue;

            _logger.LogInformation(
                "Cache HIT! Query: '{Query}' matched '{Cached}' (similarity: {Score:F3})",
                question,
                cachedQuestion,
                bestMatch.Score
            );

            return new CachedRagResponse
            {
                OriginalQuestion = cachedQuestion,
                Answer = answer,
                SimilarityScore = bestMatch.Score,
                HitCount = hitCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching cache");
            return null;
        }
    }
}
