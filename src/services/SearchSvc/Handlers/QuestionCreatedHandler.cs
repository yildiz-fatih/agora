using Contracts;
using Meilisearch;
using SearchSvc.Models;
using Wolverine;

namespace SearchSvc.Handlers;

public class QuestionCreatedHandler : IWolverineHandler
{
    private MeilisearchClient meiliClient;

    public QuestionCreatedHandler(MeilisearchClient meiliClient)
    {
        this.meiliClient = meiliClient;
    }

    public async Task HandleAsync(QuestionCreated message)
    {
        var question = new SearchQuestion
        {
            Id = message.Id,
            Title = message.Title,
            Body = message.Body,
            Tags = message.Tags,
            CreatedAt = message.CreatedAt
        };
        var createTask = await meiliClient.Index("questions").AddDocumentsAsync<SearchQuestion>([question]);
        await meiliClient.WaitForTaskAsync(createTask.TaskUid);
    }
}