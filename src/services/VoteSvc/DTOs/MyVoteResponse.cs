namespace VoteSvc.DTOs;

public record MyVoteResponse(
    Guid TargetId,
    string TargetType,
    int Value
);
