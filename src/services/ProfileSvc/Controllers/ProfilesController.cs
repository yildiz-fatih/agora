using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileSvc.Data;
using ProfileSvc.DTOs;
using ProfileSvc.Helpers;

namespace ProfileSvc.Controllers;

[ApiController]
[Route("profiles")]
public class ProfilesController : ControllerBase
{
    private readonly ProfileDbContext dbContext;

    public ProfilesController(ProfileDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        if (!AuthHelpers.TryGetUserId(User, out var userId))
        {
            return BadRequest("User ID is missing or invalid");
        }

        var profile = await dbContext.Profiles.FindAsync(userId);
        if (profile is null) { return NotFound("Profile not found"); }

        var profileResponse = new ProfileResponse(
            profile.Id,
            profile.Username,
            profile.Bio,
            profile.CreatedAt
        );

        return Ok(profileResponse);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var profile = await dbContext.Profiles.FindAsync(id);
        if (profile is null) { return NotFound("Profile not found"); }

        var profileResponse = new ProfileResponse(
            profile.Id,
            profile.Username,
            profile.Bio,
            profile.CreatedAt
        );

        return Ok(profileResponse);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var profileResponses = await dbContext.Profiles.Select(profile => new ProfileResponse(
            profile.Id,
            profile.Username,
            profile.Bio,
            profile.CreatedAt
        )).ToListAsync();

        return Ok(profileResponses);
    }

}