using Contracts;
using Microsoft.EntityFrameworkCore;
using VoteSvc.Data;
using VoteSvc.Models;
using Wolverine;

namespace VoteSvc.Handlers;

public class QuestionCreatedHandler : IWolverineHandler
{
    private VoteDbContext dbContext;

    public QuestionCreatedHandler(VoteDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task HandleAsync(QuestionCreated message)
    {
        await dbContext.LocalVoteTargets
            .Upsert(new LocalVoteTarget { TargetId = message.Id, TargetType = VoteTargetType.Question })
            .On(t => new { t.TargetId, t.TargetType })
            .NoUpdate()
            .RunAsync();
    }
}
