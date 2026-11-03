namespace BookSpace.Core.Validation;

public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 100;

    /// <summary>აბრუნებს შეცდომის ტექსტს ან null-ს, თუ პაროლი ვალიდურია.</summary>
    public static string? Validate(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinLength)
        {
            return $"პაროლი მინიმუმ {MinLength} სიმბოლო უნდა იყოს.";
        }

        if (password.Length > MaxLength)
        {
            return $"პაროლი მაქსიმუმ {MaxLength} სიმბოლო უნდა იყოს.";
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
        {
            return "პაროლი უნდა შეიცავდეს მინიმუმ ერთ ასოს და ერთ ციფრს.";
        }

        return null;
    }
}
