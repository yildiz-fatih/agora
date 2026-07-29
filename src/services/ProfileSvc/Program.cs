using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using ProfileSvc.Data;
using ProfileSvc.Middleware;

namespace ProfileSvc;

public class Program
{
    public static void Main(string[] args)
    {
        DotNetEnv.Env.Load();

        var keycloakAuthority = RequireEnv("KEYCLOAK_AUTHORITY");
        var keycloakIssuer = RequireEnv("KEYCLOAK_ISSUER");
        var keycloakAudience = RequireEnv("KEYCLOAK_AUDIENCE");
        var postgresUrl = RequireEnv("POSTGRES_URL");

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
        builder.Services.AddDbContext<ProfileDbContext>(options =>
            options.UseNpgsql(postgresUrl).UseSnakeCaseNamingConvention());

        var app = builder.Build();
        
        /*
         * Apply migrations on startup
         */
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
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
        app.UseMiddleware<ProfileCreationMiddleware>();
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