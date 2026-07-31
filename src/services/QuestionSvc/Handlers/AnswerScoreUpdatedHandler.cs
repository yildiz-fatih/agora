using Contracts;
using Microsoft.EntityFrameworkCore;
using QuestionSvc.Data;
using Wolverine;

namespace QuestionSvc.Handlers;

public class AnswerScoreUpdatedHandler : IWolverineHandler
{
    private QuestionDbContext dbContext;

    public AnswerScoreUpdatedHandler(QuestionDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task HandleAsync(AnswerScoreUpdated message)
    {
        await dbContext.Answers
            .Where(a => a.Id == message.AnswerId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.Score, message.Score));
    }
}
