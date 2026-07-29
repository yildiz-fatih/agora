using System.Security.Claims;

namespace ProfileSvc.Helpers;

public static class AuthHelpers
{
    public static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        userId = Guid.Empty;
        
        var userIdString = user.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out userId))
        {
            return false;
        }
        
        return true;
    }
}