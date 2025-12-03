namespace Application.Services;

public interface IRagService
{
    // Bước 1: Truy xuất (Retrieval) - Tìm Context liên quan
    Task<List<string>> RetrieveContextAsync(string query, int k);
    
    // Bước 2: Tạo Phản hồi (Generation) - Tổng hợp câu trả lời
    IAsyncEnumerable<string> GenerateAnswerStreamAsync(string userQuery, List<string> context);
}