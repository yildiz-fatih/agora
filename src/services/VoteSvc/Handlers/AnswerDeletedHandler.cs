using Contracts;
using Microsoft.EntityFrameworkCore;
using VoteSvc.Data;
using VoteSvc.Models;
using Wolverine;

namespace VoteSvc.Handlers;

public class AnswerDeletedHandler : IWolverineHandler
{
    private VoteDbContext dbContext;

    public AnswerDeletedHandler(VoteDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task HandleAsync(AnswerDeleted message)
    {
        await dbContext.Votes
            .Where(v => v.TargetId == message.Id && v.TargetType == VoteTargetType.Answer)
            .ExecuteDeleteAsync();
        
        await dbContext.LocalVoteTargets
            .Where(t => t.TargetId == message.Id && t.TargetType == VoteTargetType.Answer)
            .ExecuteDeleteAsync();
    }
}
