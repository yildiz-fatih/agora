namespace QuestionSvc.DTOs;

public record AnswerResponse(
    Guid Id,
    string Body,
    int Score,
    DateTime CreatedAt,
    Guid AuthorId,
    Guid QuestionId
);
