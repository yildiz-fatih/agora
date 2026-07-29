namespace ProfileSvc.DTOs;

public record ProfileResponse(
    Guid Id,
    string Username,
    string Bio,
    DateTime CreatedAt
);
