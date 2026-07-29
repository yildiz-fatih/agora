namespace ProfileSvc.Models;

public class Profile
{
    public required Guid Id { get; init; }          // "sub" claim
    public required string Username { get; init; }  // "preferred_username" claim
    public string Bio { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
