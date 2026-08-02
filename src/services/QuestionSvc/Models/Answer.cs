namespace QuestionSvc.Models;
    
public class Answer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Body { get; set; }
    public int Score { get; init; } = 0; // DO NOT SET THIS DIRECTLY, owned by VoteSvc, kept in sync with events
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public required Guid AuthorId { get; init; }
    public required string AuthorUsername { get; init; }
    
    public required Guid QuestionId { get; init; }
    public Question? Question { get; init; }
}