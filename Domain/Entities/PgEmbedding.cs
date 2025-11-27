namespace Domain.Entities;

public class PgEmbedding
{
    public Guid Id { get; set; } 
    public string Document { get; set; } // Nội dung chunk (Context)
    public float[] Embedding { get; set; } // Vector (Mảng float)
    public Guid CollectionId { get; set; }
}