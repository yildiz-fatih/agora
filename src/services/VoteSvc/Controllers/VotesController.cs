using System.Security.Claims;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoteSvc.Data;
using VoteSvc.DTOs;
using VoteSvc.Models;
using Wolverine;

namespace VoteSvc.Controllers;

[Authorize]
[ApiController]
[Route("votes")]
public class VotesController : ControllerBase
{
    private VoteDbContext dbContext;
    private IMessageBus messageBus;

    public VotesController(VoteDbContext dbContext, IMessageBus messageBus)
    {
        this.dbContext = dbContext;
        this.messageBus = messageBus;
    }
    
    [HttpPut("questions/{questionId}")]
    public async Task<IActionResult> VoteQuestion([FromRoute] Guid questionId, [FromBody] VoteRequest request)
    {
        return await SetVote(questionId, VoteTargetType.Question, request);
    }
    
    [HttpPut("answers/{answerId}")]
    public async Task<IActionResult> VoteAnswer([FromRoute] Guid answerId, [FromBody] VoteRequest request)
    {
        return await SetVote(answerId, VoteTargetType.Answer, request);
    }

    [HttpDelete("questions/{questionId}")]
    public async Task<IActionResult> ClearQuestionVote([FromRoute] Guid questionId)
    {
        return await ClearVote(questionId, VoteTargetType.Question);
    }

    [HttpDelete("answers/{answerId}")]
    public async Task<IActionResult> ClearAnswerVote([FromRoute] Guid answerId)
    {
        return await ClearVote(answerId, VoteTargetType.Answer);
    }

    private async Task<IActionResult> SetVote(Guid targetId, VoteTargetType targetType, VoteRequest request)
    {
        if (!TryGetVoterId(User, out var voterId))
        {
            return Unauthorized("User ID is missing or invalid");
        }
        
        // value must be exactly +1 or -1
        if (request.Value != 1 && request.Value != -1)
        {
            return BadRequest("Value must be 1 or -1"); 
        }
        
        // TODO (possible "bug"): look up targetId in a local table (NO sync coupling!) -> 404 if not found
        
        // Upsert the vote row (try to insert, if it exists -> update the value)
        await dbContext.Votes
            .Upsert(new Vote
                { VoterId = voterId, TargetId = targetId, TargetType = targetType, Value = request.Value })
            .On(v => new { v.VoterId, v.TargetId, v.TargetType })
            .WhenMatched(v => new Vote {Value = request.Value})
            .RunAsync();
        
        var score = await GetScore(targetId, targetType);
        
        // TODO: transactional outbox for db upsert and msg publish
        if (targetType == VoteTargetType.Question)
        {
            await messageBus.PublishAsync(new QuestionScoreUpdated(targetId, score));    
        }
        else if (targetType == VoteTargetType.Answer)
        {
            await messageBus.PublishAsync(new AnswerScoreUpdated(targetId, score));
        }
        
        return Ok(new VoteResponse(targetId, targetType.ToString(), score));
    }

    private async Task<IActionResult> ClearVote(Guid targetId, VoteTargetType targetType)
    {
        if (!TryGetVoterId(User, out var voterId))
        {
            return Unauthorized("User ID is missing or invalid");
        }
        
        // TODO (possible "bug"): look up targetId in a local table (NO sync coupling!) -> 404 if not found
        
        // Delete the matching row, do nothing if no such row exists
        await dbContext.Votes
            .Where(v => v.VoterId == voterId && v.TargetId == targetId && v.TargetType == targetType)
            .ExecuteDeleteAsync();
        
        var score = await GetScore(targetId, targetType);
        
        // TODO: transactional outbox for db delete and msg publish
        if (targetType == VoteTargetType.Question)
        {
            await messageBus.PublishAsync(new QuestionScoreUpdated(targetId, score));
        }
        else if (targetType == VoteTargetType.Answer)
        {
            await messageBus.PublishAsync(new AnswerScoreUpdated(targetId, score));
        }
        
        return Ok(new VoteResponse(targetId, targetType.ToString(), score));
    }
 
    private static bool TryGetVoterId(ClaimsPrincipal user, out Guid voterId)
    {
        voterId = Guid.Empty;
        
        var voterIdString = user.FindFirstValue("sub");
        if (string.IsNullOrEmpty(voterIdString) || !Guid.TryParse(voterIdString, out voterId))
        {
            return false;
        }
        
        return true;
    }

    private async Task<int> GetScore(Guid targetId, VoteTargetType targetType)
    {
        // SUM over zero rows in SQL is NULL, not 0 (zero) -> happens when last vote is cleared
        return await dbContext.Votes
            .Where(v => v.TargetId == targetId && v.TargetType == targetType)
            .SumAsync(v => (int?)v.Value) ?? 0;
    }
    
}
