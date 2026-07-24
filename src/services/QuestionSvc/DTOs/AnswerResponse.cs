namespace QuestionSvc.DTOs;

public record AnswerResponse(
    Guid Id,
    string Body,
    DateTime CreatedAt,
    Guid AuthorId,
    Guid QuestionId
);
