namespace BuildingBlocks.Settings;

public class QdrantSettings
{
    public string Host { get; set; } 
    public int Port { get; set; } 
    public ulong VectorSize { get; set;  }
    public string CollectionName { get; set; }
}