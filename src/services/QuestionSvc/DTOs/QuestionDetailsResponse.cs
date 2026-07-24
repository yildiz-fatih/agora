namespace QuestionSvc.DTOs;

public record QuestionDetailsResponse(
    Guid Id,
    string Title,
    string Body,
    DateTime CreatedAt,
    Guid AuthorId,
    List<string> Tags,
    ICollection<AnswerResponse> Answers
);
