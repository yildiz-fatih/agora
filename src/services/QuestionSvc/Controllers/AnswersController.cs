using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionSvc.Data;
using QuestionSvc.DTOs;
using QuestionSvc.Helpers;
using QuestionSvc.Models;
using Wolverine.EntityFrameworkCore;

namespace QuestionSvc.Controllers;

[Authorize]
[ApiController]
[Route("questions/{questionId}/answers")]
public class AnswersController : ControllerBase
{
    private QuestionDbContext dbContext;
    private IDbContextOutbox<QuestionDbContext> outbox;

    public AnswersController(QuestionDbContext dbContext, IDbContextOutbox<QuestionDbContext> outbox)
    {
        this.dbContext = dbContext;
        this.outbox = outbox;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAnswer([FromRoute] Guid questionId, [FromBody] CreateAnswerRequest request)
    {
        if (!AuthHelpers.TryGetAuthorId(User, out var authorId))
        {
            return BadRequest("Author ID is missing or invalid");
        }
        
        if (!AuthHelpers.TryGetAuthorUsername(User, out var authorUsername))
        {
            return BadRequest("Author username is missing or invalid");
        }
        
        var questionExists = await outbox.DbContext.Questions.AnyAsync(q => q.Id == questionId);
        if (!questionExists)
        {
            return NotFound($"Question with id {questionId} not found");
        }
        
        var answer = new Answer
        {
            Body = request.Body,
            AuthorId = authorId,
            AuthorUsername = authorUsername,
            QuestionId = questionId
        };

        outbox.DbContext.Answers.Add(answer);
        
        await outbox.PublishAsync(new AnswerCreated(answer.Id, answer.QuestionId));

        await outbox.SaveChangesAndFlushMessagesAsync();
        
        var answerResponse = new AnswerResponse(answer.Id, answer.Body, answer.Score, answer.CreatedAt, answer.AuthorId,
            answer.AuthorUsername, answer.QuestionId);
        
        return StatusCode(StatusCodes.Status201Created, answerResponse);
    }
    
    [HttpPut("{answerId}")]
    public async Task<IActionResult> UpdateAnswer(
        [FromRoute] Guid questionId,
        [FromRoute] Guid answerId,
        [FromBody] UpdateAnswerRequest request)
    {
        if (!AuthHelpers.TryGetAuthorId(User, out var authorId))
        {
            return BadRequest("Author ID is missing or invalid");
        }
        
        var answer = await dbContext.Answers.FindAsync(answerId);
        if (answer is null)
        {
            return NotFound($"Answer with id {answerId} not found");
        }

        if (answer.QuestionId != questionId)
        {
            return BadRequest($"Answer with id {answerId} does not belong to question with id {questionId}");
        }
        
        if (answer.AuthorId != authorId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "You are not the author of this answer");
        }

        answer.Body = request.Body;
        await dbContext.SaveChangesAsync();
        
        return NoContent();
    }
    
    [HttpDelete("{answerId}")]
    public async Task<IActionResult> DeleteAnswer([FromRoute] Guid questionId, [FromRoute] Guid answerId)
    {
        if (!AuthHelpers.TryGetAuthorId(User, out var authorId))
        {
            return BadRequest("Author ID is missing or invalid");
        }
        
        var answer = await outbox.DbContext.Answers.FindAsync(answerId);
        if (answer is null)
        {
            return NotFound($"Answer with id {answerId} not found");
        }

        if (answer.QuestionId != questionId)
        {
            return BadRequest($"Answer with id {answerId} does not belong to question with id {questionId}");
        }
        
        if (answer.AuthorId != authorId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "You are not the author of this answer");
        }

        outbox.DbContext.Answers.Remove(answer);
        await outbox.PublishAsync(new AnswerDeleted(answer.Id));
        await outbox.SaveChangesAndFlushMessagesAsync();
        
        return NoContent();
    }
    
}
