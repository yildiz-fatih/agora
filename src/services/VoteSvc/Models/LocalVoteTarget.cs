namespace VoteSvc.Models;

public class LocalVoteTarget
{
    public Guid TargetId { get; set; }
    public VoteTargetType TargetType { get; set; }
    public Guid? ParentQuestionId  { get; set; } // null for questions, set for answers
}
