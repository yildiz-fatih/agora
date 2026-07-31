using Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using QuestionSvc.Data;
using Wolverine;
using Wolverine.RabbitMQ;

namespace QuestionSvc;

public class Program
{
    public static void Main(string[] args)
    {
        DotNetEnv.Env.Load();

        var keycloakAuthority = RequireEnv("KEYCLOAK_AUTHORITY");
        var keycloakIssuer = RequireEnv("KEYCLOAK_ISSUER");
        var keycloakAudience = RequireEnv("KEYCLOAK_AUDIENCE");
        var postgresUrl = RequireEnv("POSTGRES_URL");
        var rabbitmqUrl = RequireEnv("RABBITMQ_URL");

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakAuthority;
                options.Audience = keycloakAudience;
                options.RequireHttpsMetadata = false;
                options.MapInboundClaims = false;
                options.TokenValidationParameters.ValidIssuer = keycloakIssuer;
            });
        builder.Services.AddDbContext<QuestionDbContext>(
            options => options.UseNpgsql(postgresUrl).UseSnakeCaseNamingConvention(),
            optionsLifetime: ServiceLifetime.Singleton); // Wolverine needs this to build handlers that use EF
        builder.Host.UseWolverine(options =>
        {
            options.UseRabbitMq(new Uri(rabbitmqUrl)).AutoProvision();
            
            options.PublishMessage<QuestionCreated>().ToRabbitExchange("questions");
            options.PublishMessage<QuestionUpdated>().ToRabbitExchange("questions");
            options.PublishMessage<QuestionDeleted>().ToRabbitExchange("questions");
            
            options.ListenToRabbitQueue("questionsvc-votes", listenOptions =>
            {
                listenOptions.BindExchange("votes");
            });
        });
        
        var app = builder.Build();

        /*
         * Apply migrations on startup
         * TODO: Replace this when QuestionSvc is dockerized
         */
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<QuestionDbContext>();
            try
            {
                db.Database.Migrate();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to migrate database: {e.Message}");
                throw;
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

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
