using Microsoft.AspNetCore.Authentication.JwtBearer;

DotNetEnv.Env.Load();

var keycloakAuthority = Environment.GetEnvironmentVariable("KEYCLOAK_AUTHORITY");
if (string.IsNullOrWhiteSpace(keycloakAuthority))
{
    Console.Error.WriteLine("KEYCLOAK_AUTHORITY is not set");
    Environment.Exit(1);
}

var keycloakAudience = Environment.GetEnvironmentVariable("KEYCLOAK_AUDIENCE");
if (string.IsNullOrWhiteSpace(keycloakAudience))
{
    Console.Error.WriteLine("KEYCLOAK_AUDIENCE is not set");
    Environment.Exit(1);
}

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
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
