using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionSvc.Data;
using QuestionSvc.DTOs;
using QuestionSvc.Models;

namespace QuestionSvc.Controllers;

[ApiController]
[Route("questions")]
public class QuestionsController : ControllerBase
{
    private QuestionDbContext dbContext;

    public QuestionsController(QuestionDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        var authorIdString = User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(authorIdString))
        {
            return BadRequest("Author ID is missing");
        }

        if (!Guid.TryParse(authorIdString, out var authorIdGuid))
        {
            return BadRequest("Author ID is not a Guid");
        }
        var question = new Question
        {
            Title = request.Title,
            Body = request.Body,
            AuthorId = authorIdGuid,
            Tags = request.Tags ?? []
        };
        
        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync();

        var questionResponse = new QuestionResponse(question.Id, question.Title, question.Body, question.CreatedAt,
            question.AuthorId, question.Tags);
        
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
            .Select(q => new QuestionResponse(q.Id, q.Title, q.Body, q.CreatedAt, q.AuthorId, q.Tags)).ToListAsync();
        
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
            .Select(a => new AnswerResponse(a.Id, a.Body, a.CreatedAt, a.AuthorId, a.QuestionId)).ToList();

        var questionDetailsResponse = new QuestionDetailsResponse(question.Id, question.Title, question.Body,
            question.CreatedAt, question.AuthorId, question.Tags, answersResponse);
        
        return Ok(questionDetailsResponse);
    }
    
}