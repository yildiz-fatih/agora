namespace Contracts;

public record QuestionUpdated(
    Guid Id,
    string Title,
    string Body,
    List<string> Tags
);
