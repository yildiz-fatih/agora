namespace Contracts;

public record AnswerScoreUpdated(
    Guid AnswerId,
    int Score
);
