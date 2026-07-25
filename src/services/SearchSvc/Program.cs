using Microsoft.AspNetCore.Mvc;

namespace SearchSvc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }

        app.MapGet("/search", ([FromQuery] string q, [FromQuery] string? tags) =>
        {
            return Results.Ok("hello world");
        });

        app.Run();
        
    }
}
