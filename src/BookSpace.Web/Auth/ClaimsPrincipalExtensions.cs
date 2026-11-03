using System.Security.Claims;
using BookSpace.Core.Domain;

namespace BookSpace.Web.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public static UserRole GetRole(this ClaimsPrincipal user) =>
        Enum.TryParse<UserRole>(user.FindFirstValue(ClaimTypes.Role), out var role) ? role : UserRole.Student;

    public static ClaimsPrincipal ToPrincipal(this User user, string authenticationType)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));
    }
}
