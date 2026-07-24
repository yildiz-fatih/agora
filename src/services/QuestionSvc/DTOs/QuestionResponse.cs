namespace QuestionSvc.DTOs;

public record QuestionResponse(
    Guid Id,
    string Title,
    string Body,
    DateTime CreatedAt,
    Guid AuthorId,
    List<string> Tags
);
