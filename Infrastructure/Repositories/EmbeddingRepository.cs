using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;

namespace Infrastructure.Repositories;

public interface IEmbeddingRepository
{
    /// <summary>
    /// Tìm kiếm các document tương tự dựa trên vector embedding sử dụng cosine similarity
    /// </summary>
    /// <param name="queryEmbedding">Vector embedding của câu query</param>
    /// <param name="k">Số lượng kết quả trả về</param>
    /// <returns>Danh sách các document content tương tự nhất</returns>
    Task<List<string>> SearchSimilarDocumentsAsync(float[] queryEmbedding, int k);
}

public class EmbeddingRepository(AppDbContext dbContext) : IEmbeddingRepository
{
    public async Task<List<string>> SearchSimilarDocumentsAsync(float[] queryEmbedding, int k)
    {
        var vector = new Vector(queryEmbedding);
        
        // Sử dụng raw SQL với pgvector cosine distance operator (<=>)
        // Cosine distance = 1 - cosine_similarity, nên ORDER BY ASC để lấy kết quả tương tự nhất
        var sql = @"
            SELECT document 
            FROM langchain_pg_embedding 
            ORDER BY embedding <=> @embedding::vector
            LIMIT @limit";
        
        var results = await dbContext.Database
            .SqlQueryRaw<string>(sql, 
                new NpgsqlParameter("@embedding", vector.ToString()),
                new NpgsqlParameter("@limit", k))
            .ToListAsync();
        
        return results;
    }
}
