using BookSpace.Core.Abstractions;
using BookSpace.Core.Common;
using BookSpace.Core.Domain;
using BookSpace.Core.Rules;
using BookSpace.Core.Validation;

namespace BookSpace.Core.Services;

/// <summary>US-01 რეგისტრაცია, US-02 შესვლა.</summary>
public class AuthService
{
    public const string InvalidCredentialsMessage = "ელფოსტა ან პაროლი არასწორია.";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;
    private readonly AccountRules _rules;

    public AuthService(IUserRepository users, IPasswordHasher hasher, IClock clock, AccountRules rules)
    {
        _users = users;
        _hasher = hasher;
        _clock = clock;
        _rules = rules;
    }

    public async Task<Result<User>> RegisterAsync(string? email, string? password, CancellationToken ct = default)
    {
        var normalized = EmailValidator.Normalize(email);
        if (!EmailValidator.IsUniversityEmail(normalized, _rules.AllowedEmailDomain))
        {
            return Result<User>.Fail(ErrorCode.Validation,
                $"გამოიყენეთ უნივერსიტეტის ელფოსტა (@...{_rules.AllowedEmailDomain}).");
        }

        var passwordError = PasswordPolicy.Validate(password);
        if (passwordError is not null)
        {
            return Result<User>.Fail(ErrorCode.Validation, passwordError);
        }

        if (await _users.EmailExistsAsync(normalized, ct))
        {
            return Result<User>.Fail(ErrorCode.Conflict, "ეს ელფოსტა უკვე რეგისტრირებულია.");
        }

        var user = new User
        {
            Email = normalized,
            PasswordHash = _hasher.Hash(password!),
            Role = UserRole.Student,
            CreatedAt = _clock.Now
        };

        // ბაზის unique index იცავს ერთდროული რეგისტრაციისგან.
        if (!await _users.TryAddAsync(user, ct))
        {
            return Result<User>.Fail(ErrorCode.Conflict, "ეს ელფოსტა უკვე რეგისტრირებულია.");
        }

        return Result<User>.Ok(user);
    }

    public async Task<Result<User>> LoginAsync(string? email, string? password, CancellationToken ct = default)
    {
        var normalized = EmailValidator.Normalize(email);
        if (normalized.Length == 0 || string.IsNullOrEmpty(password))
        {
            return Result<User>.Fail(ErrorCode.Unauthorized, InvalidCredentialsMessage);
        }

        var user = await _users.GetByEmailAsync(normalized, ct);

        // ერთი და იგივე შეტყობინება ორივე შემთხვევაში, რომ არ გამჟღავნდეს, ელფოსტა არსებობს თუ არა.
        if (user is null || !_hasher.Verify(user.PasswordHash, password))
        {
            return Result<User>.Fail(ErrorCode.Unauthorized, InvalidCredentialsMessage);
        }

        return Result<User>.Ok(user);
    }
}
