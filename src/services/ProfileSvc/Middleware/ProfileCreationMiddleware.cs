using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProfileSvc.Data;
using ProfileSvc.Helpers;
using ProfileSvc.Models;

namespace ProfileSvc.Middleware;

public class ProfileCreationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ProfileDbContext dbContext)
    {
        if (context.User.Identity == null || context.User.Identity.IsAuthenticated == false)
        {
            await next(context);
            return;
        }
        
        if (!AuthHelpers.TryGetUserId(context.User, out var userId))
        {
            await next(context);
            return;
        }
        
        var username = context.User.FindFirstValue("preferred_username");
        if (username is null)
        {
            await next(context);
            return;
        }
        
        var newProfile = new Profile { Id = userId, Username = username };
        await dbContext.Profiles.Upsert(newProfile).On(p => p.Id).NoUpdate().RunAsync();
        
        await next(context);
    }
}
