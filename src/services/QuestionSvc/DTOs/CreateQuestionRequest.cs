using System.ComponentModel.DataAnnotations;

namespace QuestionSvc.DTOs;

public record CreateQuestionRequest(
    [Required] string Title,
    [Required] string Body,
    List<string>? Tags
);
