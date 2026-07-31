using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionSvc.Data;
using QuestionSvc.DTOs;
using QuestionSvc.Helpers;
using QuestionSvc.Models;
using Wolverine;

namespace QuestionSvc.Controllers;

[ApiController]
[Route("questions")]
public class QuestionsController : ControllerBase
{
    private QuestionDbContext dbContext;
    private IMessageBus messageBus;

    public QuestionsController(QuestionDbContext dbContext, IMessageBus messageBus)
    {
        this.dbContext = dbContext;
        this.messageBus = messageBus;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        if (!AuthHelpers.TryGetAuthorId(User, out var authorId))
        {
            return BadRequest("Author ID is missing or invalid");
        }
        
        var question = new Question
        {
            Title = request.Title,
            Body = request.Body,
            AuthorId = authorId,
            Tags = request.Tags ?? []
        };
        
        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync();

        /* TODO: transactional outbox - make the msg publish and DB save part of the same transaction */
        var questionCreatedMsg =
            new QuestionCreated(question.Id, question.Title, question.Body, question.Tags, question.CreatedAt);
        await messageBus.PublishAsync(questionCreatedMsg);

        var questionResponse = new QuestionResponse(question.Id, question.Title, question.Body, question.Score,
            question.CreatedAt, question.AuthorId, question.Tags);
        
        return Created($"/questions/{question.Id}", questionResponse);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllQuestions([FromQuery] string? tag)
    {
        var query = dbContext.Questions.AsQueryable();

        if (!string.IsNullOrEmpty(tag))
        {
            query = query.Where(q => q.Tags.Contains(tag));
        }

        var questionsResponse = await query.OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuestionResponse(q.Id, q.Title, q.Body, q.Score, q.CreatedAt, q.AuthorId, q.Tags)).ToListAsync();
        
        return Ok(questionsResponse);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuestionById([FromRoute] Guid id)
    {
        var question = await dbContext.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);
        if (question is null)
        {
            return NotFound($"Question with id {id} not found");
        }

        var answersResponse = question.Answers
            .OrderByDescending(a => a.Score)
            .ThenBy(a => a.CreatedAt)
            .Select(a => new AnswerResponse(a.Id, a.Body, a.Score, a.CreatedAt, a.AuthorId, a.QuestionId))
            .ToList();

        var questionDetailsResponse = new QuestionDetailsResponse(question.Id, question.Title, question.Body,
            question.Score, question.CreatedAt, question.AuthorId, question.Tags, answersResponse);
        
        return Ok(questionDetailsResponse);
    }
    
    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateQuestion([FromRoute] Guid id, [FromBody] UpdateQuestionRequest request)
    {
        if (!AuthHelpers.TryGetAuthorId(User, out var authorId))
        {
            return BadRequest("Author ID is missing or invalid");
        }
        
        var question = await dbContext.Questions.FindAsync(id);
        if (question is null)
        {
            return NotFound($"Question with id {id} not found");
        }
        
        if (!authorId.Equals(question.AuthorId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "You are not the author of this question");
        }

        if (!string.IsNullOrEmpty(request.Title)) question.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Body)) question.Body = request.Body;
        if (request.Tags is not null) question.Tags = request.Tags;
        
        await dbContext.SaveChangesAsync();
        
        var questionUpdatedMsg = new QuestionUpdated(question.Id, question.Title, question.Body, question.Tags);
        await messageBus.PublishAsync(questionUpdatedMsg);
        
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuestion([FromRoute] Guid id)
    {
        if (!AuthHelpers.TryGetAuthorId(User, out var authorId))
        {
            return BadRequest("Author ID is missing or invalid");
        }
        
        var question = await dbContext.Questions.FindAsync(id);
        if (question is null)
        {
            return NotFound($"Question with id {id} not found");
        }
        
        if (!authorId.Equals(question.AuthorId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "You are not the author of this question");
        }

        dbContext.Remove(question);
        await dbContext.SaveChangesAsync();
        
        var questionDeletedMsg = new QuestionDeleted(question.Id);
        await messageBus.PublishAsync(questionDeletedMsg);
        
        return NoContent();
    }


    
}