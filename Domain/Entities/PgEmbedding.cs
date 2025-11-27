using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace Domain.Entities;

[Table("langchain_pg_embedding")]
public class PgEmbedding
{
    [Column("id")]
    public Guid Id { get; set; } 
    
    [Column("collection_id")]
    public Guid CollectionId { get; set; }
    
    [Column("embedding")]
    public Vector Embedding { get; set; } = null!; // Vector type từ pgvector
    
    [Column("document")]
    public string Document { get; set; } = string.Empty; // Nội dung chunk (Context)
    
    [Column("cmetadata")]
    public string? CMetadata { get; set; } // Metadata JSON
}