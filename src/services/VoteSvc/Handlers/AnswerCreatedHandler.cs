using Contracts;
using Microsoft.EntityFrameworkCore;
using VoteSvc.Data;
using VoteSvc.Models;
using Wolverine;

namespace VoteSvc.Handlers;

public class AnswerCreatedHandler : IWolverineHandler
{
    private VoteDbContext dbContext;

    public AnswerCreatedHandler(VoteDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task HandleAsync(AnswerCreated message)
    {
        await dbContext.LocalVoteTargets
            .Upsert(new LocalVoteTarget
            {
                TargetId = message.Id,
                TargetType = VoteTargetType.Answer,
                ParentQuestionId = message.QuestionId
            })
            .On(t => new { t.TargetId, t.TargetType })
            .NoUpdate()
            .RunAsync();
    }
}
