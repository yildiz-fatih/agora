namespace QuestionSvc.Models;

public class Answer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Body { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public required Guid AuthorId { get; init; }
    
    public required Guid QuestionId { get; init; }
    public Question? Question { get; init; }
}