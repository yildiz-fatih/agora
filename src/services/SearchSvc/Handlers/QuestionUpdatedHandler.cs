using Contracts;
using Meilisearch;
using Wolverine;

namespace SearchSvc.Handlers;

public class QuestionUpdatedHandler : IWolverineHandler
{
    private record SearchQuestionUpdate(Guid Id, string Title, string Body, List<string> Tags);

    private MeilisearchClient meiliClient;

    public QuestionUpdatedHandler(MeilisearchClient meiliClient)
    {
        this.meiliClient = meiliClient;
    }

    public async Task HandleAsync(QuestionUpdated message)
    {
        var questionUpdate = new SearchQuestionUpdate(message.Id, message.Title, message.Body, message.Tags);
        var updateTask = await meiliClient.Index("questions").UpdateDocumentsAsync([questionUpdate]);
        await meiliClient.WaitForTaskAsync(updateTask.TaskUid);
    }
}