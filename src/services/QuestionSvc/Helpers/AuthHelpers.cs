using System.Security.Claims;

namespace QuestionSvc.Helpers;

public static class AuthHelpers
{
    public static bool TryGetAuthorId(ClaimsPrincipal user, out Guid authorId)
    {
        authorId = Guid.Empty;
        
        var authorIdString = user.FindFirstValue("sub");
        if (string.IsNullOrEmpty(authorIdString) || !Guid.TryParse(authorIdString, out authorId))
        {
            return false;
        }
        
        return true;
    }
    
    public static bool TryGetAuthorUsername(ClaimsPrincipal user, out string authorUsername)
    {
        authorUsername = string.Empty;

        var value = user.FindFirstValue("preferred_username");
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        authorUsername = value;
        return true;
    }
}