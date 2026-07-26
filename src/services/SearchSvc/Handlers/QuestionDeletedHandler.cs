using Contracts;
using Meilisearch;
using Wolverine;

namespace SearchSvc.Handlers;

public class QuestionDeletedHandler : IWolverineHandler
{
    private MeilisearchClient meiliClient;

    public QuestionDeletedHandler(MeilisearchClient meiliClient)
    {
        this.meiliClient = meiliClient;
    }

    public async Task HandleAsync(QuestionDeleted message)
    {
        var deleteTask = await meiliClient.Index("questions").DeleteOneDocumentAsync(message.Id.ToString());
        await meiliClient.WaitForTaskAsync(deleteTask.TaskUid);
    }
}