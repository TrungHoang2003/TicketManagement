namespace BuildingBlocks.Settings;

public class OllamaSettings
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string EmbeddingModel { get; set; } = "nomic-embed-text";
    public string GenerationModel { get; set; } = "llama3";
    public int EmbeddingDimension { get; set; } = 768;
}

public class RagPromptSettings
{
    public string SystemInstruction { get; set; } = string.Empty;
}
