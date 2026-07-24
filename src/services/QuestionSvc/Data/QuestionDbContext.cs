using Microsoft.EntityFrameworkCore;
using QuestionSvc.Models;

namespace QuestionSvc.Data;

public class QuestionDbContext : DbContext
{
    public QuestionDbContext(DbContextOptions<QuestionDbContext> options) : base(options)
    {
    }
    
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
}