using VoteSvc.Models;

namespace VoteSvc.DTOs;

public record VoteResponse(
    Guid TargetId,
    string TargetType,
    int Score
);
