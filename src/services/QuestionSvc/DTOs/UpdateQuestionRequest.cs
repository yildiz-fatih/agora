namespace QuestionSvc.DTOs;
    
public record UpdateQuestionRequest(
    string? Title,
    string? Body,
    List<string>? Tags
);
