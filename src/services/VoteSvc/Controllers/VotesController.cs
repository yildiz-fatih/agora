using System.Security.Claims;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoteSvc.Data;
using VoteSvc.DTOs;
using VoteSvc.Models;
using Wolverine.EntityFrameworkCore;

namespace VoteSvc.Controllers;

[Authorize]
[ApiController]
[Route("votes")]
public class VotesController : ControllerBase
{
    private VoteDbContext dbContext;
    private IDbContextOutbox<VoteDbContext> outbox;

    public VotesController(VoteDbContext dbContext, IDbContextOutbox<VoteDbContext> outbox)
    {
        this.dbContext = dbContext;
        this.outbox = outbox;
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetMyVotes([FromQuery] Guid? questionId, [FromQuery] string? answerIds)
    {
        if (!TryGetVoterId(User, out var voterId))
        {
            return Unauthorized("User ID is missing or invalid");
        }

        var targetIds = new List<Guid>();
        if (questionId.HasValue)
        {
            targetIds.Add(questionId.Value);
        }
        if (!string.IsNullOrWhiteSpace(answerIds))
        {
            foreach (var idString in answerIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Guid.TryParse(idString, out var answerId))
                {
                    targetIds.Add(answerId);
                }
            }
        }

        if (targetIds.Count == 0)
        {
            return Ok(Array.Empty<MyVoteResponse>());
        }
        
        var myVotes = await dbContext.Votes
            .Where(v => v.VoterId == voterId && targetIds.Contains(v.TargetId))
            .Select(v => new MyVoteResponse(v.TargetId, v.TargetType.ToString(), v.Value))
            .ToListAsync();

        return Ok(myVotes);
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
        
        var exists = await outbox.DbContext.LocalVoteTargets
            .AnyAsync(t => t.TargetId == targetId && t.TargetType == targetType);
        if (!exists)
        {
            return NotFound("Target ID not found");
        }
        
        await using (var transaction = await outbox.DbContext.Database.BeginTransactionAsync())
        {
            try
            {
                // Upsert the vote row (try to insert, if it exists -> update the value)
                await outbox.DbContext.Votes
                    .Upsert(new Vote
                        { VoterId = voterId, TargetId = targetId, TargetType = targetType, Value = request.Value })
                    .On(v => new { v.VoterId, v.TargetId, v.TargetType })
                    .WhenMatched(v => new Vote {Value = request.Value})
                    .RunAsync();
        
                var score = await GetScore(outbox.DbContext, targetId, targetType);
        
                if (targetType == VoteTargetType.Question)
                {
                    await outbox.PublishAsync(new QuestionScoreUpdated(targetId, score));    
                }
                else if (targetType == VoteTargetType.Answer)
                {
                    await outbox.PublishAsync(new AnswerScoreUpdated(targetId, score));
                }

                await outbox.SaveChangesAndFlushMessagesAsync();
                // no transaction.CommitAsync() here -> SaveChangesAndFlushMessagesAsync already committed it
        
                return Ok(new VoteResponse(targetId, targetType.ToString(), score));
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
    }

    private async Task<IActionResult> ClearVote(Guid targetId, VoteTargetType targetType)
    {
        if (!TryGetVoterId(User, out var voterId))
        {
            return Unauthorized("User ID is missing or invalid");
        }
        
        var exists = await outbox.DbContext.LocalVoteTargets
            .AnyAsync(t => t.TargetId == targetId && t.TargetType == targetType);
        if (!exists)
        {
            return NotFound("Target ID not found");
        }

        await using (var transaction = await outbox.DbContext.Database.BeginTransactionAsync())
        {
            try
            {
                // Delete the matching row, do nothing if no such row exists
                await outbox.DbContext.Votes
                    .Where(v => v.VoterId == voterId && v.TargetId == targetId && v.TargetType == targetType)
                    .ExecuteDeleteAsync();
        
                var score = await GetScore(outbox.DbContext, targetId, targetType);
        
                if (targetType == VoteTargetType.Question)
                {
                    await outbox.PublishAsync(new QuestionScoreUpdated(targetId, score));
                }
                else if (targetType == VoteTargetType.Answer)
                {
                    await outbox.PublishAsync(new AnswerScoreUpdated(targetId, score));
                }
        
                await outbox.SaveChangesAndFlushMessagesAsync();
                // no transaction.CommitAsync() here -> SaveChangesAndFlushMessagesAsync already committed it
        
                return Ok(new VoteResponse(targetId, targetType.ToString(), score));
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
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

    private static async Task<int> GetScore(VoteDbContext dbContext, Guid targetId, VoteTargetType targetType)
    {
        // SUM over zero rows in SQL is NULL, not 0 (zero) -> happens when last vote is cleared
        return await dbContext.Votes
            .Where(v => v.TargetId == targetId && v.TargetType == targetType)
            .SumAsync(v => (int?)v.Value) ?? 0;
    }
    
}
