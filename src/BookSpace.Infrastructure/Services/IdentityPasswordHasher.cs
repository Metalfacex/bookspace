using BookSpace.Core.Abstractions;
using BookSpace.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace BookSpace.Infrastructure.Services;

/// <summary>ASP.NET Core Identity-ის PBKDF2 ჰეშერის ადაპტერი (Adapter pattern).</summary>
public class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly User HashContext = new();
    private readonly PasswordHasher<User> _inner = new();

    public string Hash(string password) => _inner.HashPassword(HashContext, password);

    public bool Verify(string hash, string password)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return false;
        }

        try
        {
            return _inner.VerifyHashedPassword(HashContext, hash, password) != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
