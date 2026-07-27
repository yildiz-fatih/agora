using System.ComponentModel.DataAnnotations;

namespace QuestionSvc.DTOs;

public record CreateAnswerRequest(
    [Required] string Body
);
