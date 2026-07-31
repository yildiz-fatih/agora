namespace Contracts;

public record QuestionScoreUpdated(
    Guid QuestionId,
    int Score
);
