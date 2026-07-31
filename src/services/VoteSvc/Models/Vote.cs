namespace VoteSvc.Models;


public enum VoteTargetType
{
    Question,
    Answer
}

public class Vote
{
    public Guid VoterId { get; set; }
    public Guid TargetId { get; set; }
    public VoteTargetType TargetType { get; set; }
    public int Value { get; set; }
}
