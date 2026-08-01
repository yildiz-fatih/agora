using Meilisearch;
using Microsoft.AspNetCore.Mvc;
using SearchSvc.Models;
using Wolverine;
using Wolverine.RabbitMQ;

namespace SearchSvc;

public class Program
{
    public static async Task Main(string[] args)
    {
        DotNetEnv.Env.Load();

        var meiliUrl = RequireEnv("MEILI_URL");
        var meiliKey = RequireEnv("MEILI_KEY");
        var rabbitmqUrl = RequireEnv("RABBITMQ_URL");
        var frontendUrl = RequireEnv("FRONTEND_URL");
        
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddOpenApi();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy => policy
                .WithOrigins(frontendUrl)
                .AllowAnyMethod()
                .AllowAnyHeader());
        });
        builder.Services.AddSingleton(
            new MeilisearchClient(meiliUrl, meiliKey)
        );
        builder.Host.UseWolverine(options =>
        {
            options.UseRabbitMq(new Uri(rabbitmqUrl)).AutoProvision();
            options.ListenToRabbitQueue("searchsvc-questions", listenOptions =>
            {
                listenOptions.BindExchange("questions", bindingKey: "created");
                listenOptions.BindExchange("questions", bindingKey: "updated");
                listenOptions.BindExchange("questions", bindingKey: "deleted");
            });
        });

        var app = builder.Build();
        
        // --- Meilisearch ---
        var meiliClient = app.Services.GetRequiredService<MeilisearchClient>();
        // ensure "questions" index exists
        try
        {
            await meiliClient.GetIndexAsync("questions");
        }
        catch (MeilisearchApiError e)
        {
            if (e.Code != "index_not_found") { throw; }
            
            var createTask = await meiliClient.CreateIndexAsync("questions", "id");
            await meiliClient.WaitForTaskAsync(createTask.TaskUid);
        }
        // configure Meilisearch
        var settingsTask = await meiliClient.Index("questions").UpdateSettingsAsync(new Settings
        {
            SearchableAttributes = ["title", "body"],
            FilterableAttributes = ["tags"]
        });
        await meiliClient.WaitForTaskAsync(settingsTask.TaskUid);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }
        app.UseCors("AllowFrontend");

        app.MapGet("/search", async (
            [FromQuery] string q,
            [FromQuery] string? tags,
            [FromServices] MeilisearchClient client) =>
        {
            // 400, if "q" is null or empty/whitespace
            if (string.IsNullOrWhiteSpace(q))
            {
                return Results.BadRequest("Query parameter 'q' is required");
            }
            // search options
            var searchQuery = new SearchQuery();
            // filter by tags, if any exist
            if (!string.IsNullOrWhiteSpace(tags))
            {
                // split tags by ","
                var tagList = tags.Split(",", StringSplitOptions.RemoveEmptyEntries);
                // filter by tags
                if (tagList.Length > 0)
                {
                    searchQuery.Filter = string.Join(" AND ", tagList.Select(tag => $"tags = \"{tag}\""));
                }  
            }
            // search
            var result = await client.Index("questions").SearchAsync<SearchQuestion>(q, searchQuery);
            // return
            return Results.Ok(result.Hits);
        });

        app.Run();
        
    }
    
    private static string RequireEnv(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Environment variable {name} is not set");
        }
        return value;
    }
}
