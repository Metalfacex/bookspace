namespace BookSpace.Web.Auth;

public static class Roles
{
    public const string Student = nameof(BookSpace.Core.Domain.UserRole.Student);
    public const string Admin = nameof(BookSpace.Core.Domain.UserRole.Admin);
    public const string Manager = nameof(BookSpace.Core.Domain.UserRole.Manager);
    public const string AdminOrManager = Admin + "," + Manager;
}
