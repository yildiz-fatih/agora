using Contracts;
using Microsoft.EntityFrameworkCore;
using QuestionSvc.Data;
using Wolverine;

namespace QuestionSvc.Handlers;

public class QuestionScoreUpdatedHandler : IWolverineHandler
{
    private QuestionDbContext dbContext;

    public QuestionScoreUpdatedHandler(QuestionDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task HandleAsync(QuestionScoreUpdated message)
    {
        await dbContext.Questions
            .Where(q => q.Id == message.QuestionId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(q => q.Score, message.Score));
    }
}
