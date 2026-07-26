namespace Contracts;

public record QuestionCreated(
    Guid Id,
    string Title,
    string Body,
    List<string> Tags,
    DateTime CreatedAt
);
