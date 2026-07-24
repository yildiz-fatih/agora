namespace QuestionSvc.Models;

public class Question
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Body { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public required Guid AuthorId { get; init; }
    public List<string> Tags { get; set; } = []; // postgres array
    
    public ICollection<Answer> Answers { get; init; } = [];
}