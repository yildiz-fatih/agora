namespace QuestionSvc.Models;

public class Question
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Title { get; init; }
    public required string Body { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public required Guid AuthorId { get; init; }
    public List<string> Tags { get; init; } = []; // postgres array
    
    public ICollection<Answer> Answers { get; init; } = [];
}