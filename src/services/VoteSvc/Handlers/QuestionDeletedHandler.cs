using Contracts;
using Microsoft.EntityFrameworkCore;
using VoteSvc.Data;
using VoteSvc.Models;
using Wolverine;

namespace VoteSvc.Handlers;

public class QuestionDeletedHandler : IWolverineHandler
{
    private VoteDbContext dbContext;

    public QuestionDeletedHandler(VoteDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task HandleAsync(QuestionDeleted message)
    {
        // Get IDs of the question and all of its answers
        var targetIds = await dbContext.LocalVoteTargets
            .Where(t => t.ParentQuestionId == message.Id
                        || (t.TargetId == message.Id
                            && t.TargetType == VoteTargetType.Question))
            .Select(t => t.TargetId)
            .ToListAsync();

        // Delete all votes for the targets 
        await dbContext.Votes
            .Where(v => targetIds.Contains(v.TargetId))
            .ExecuteDeleteAsync();

        // Delete all vote targets (question and its answers)
        await dbContext.LocalVoteTargets
            .Where(t => targetIds.Contains(t.TargetId))
            .ExecuteDeleteAsync();
    }
}
