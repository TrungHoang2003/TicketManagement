namespace Application.Services;

public interface IRagService
{
    // Bước 1: Truy xuất (Retrieval) - Tìm Context liên quan
    Task<List<string>> RetrieveContextAsync(string query, int k);
    
    // Bước 2: Tạo Phản hồi (Generation) - Tổng hợp câu trả lời
    Task<string> GenerateAnswerAsync(string userQuery, List<string> context);
    
    // Bước 3: Tính Embedding cho câu hỏi (để cache)
    Task<float[]> GetQuestionEmbeddingAsync(string question);
}