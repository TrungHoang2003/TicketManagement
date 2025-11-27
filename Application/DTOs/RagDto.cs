namespace Application.DTOs;

public record RagQueryDto(string Query);

public record RagResponseDto(
    string Answer, 
    List<string> SourceDocuments,
    DateTime Timestamp);
