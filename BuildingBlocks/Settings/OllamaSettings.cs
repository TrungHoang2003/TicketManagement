namespace BuildingBlocks.Settings;

public class OllamaSettings
{
    public string BaseUrl { get; set; } 
    public string EmbeddingModel { get; set; } 
    public string GenerationModel { get; set; } 
    public int EmbeddingDimension { get; set; } 
}

public class RagPromptSettings
{
    public string SystemInstruction { get; set; }
}
