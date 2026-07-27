using System.ComponentModel.DataAnnotations;

namespace QuestionSvc.DTOs;

public record UpdateAnswerRequest(
    [Required] string Body
);
