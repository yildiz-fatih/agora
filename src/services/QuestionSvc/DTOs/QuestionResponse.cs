namespace QuestionSvc.DTOs;

public record QuestionResponse(
    Guid Id,
    string Title,
    string Body,
    int Score,
    DateTime CreatedAt,
    Guid AuthorId,
    List<string> Tags
);
