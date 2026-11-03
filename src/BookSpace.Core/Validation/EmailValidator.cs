using System.Net.Mail;

namespace BookSpace.Core.Validation;

public static class EmailValidator
{
    public static string Normalize(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    /// <summary>
    /// ამოწმებს, რომ ელფოსტა ვალიდურია და ეკუთვნის უნივერსიტეტის დომენს
    /// (მაგ. suffix = "edu.ge" → student@uni.edu.ge ან student@edu.ge).
    /// </summary>
    public static bool IsUniversityEmail(string? email, string domainSuffix)
    {
        var normalized = Normalize(email);
        if (normalized.Length == 0 || normalized.Length > 254)
        {
            return false;
        }

        if (!MailAddress.TryCreate(normalized, out var address) || address.Address != normalized)
        {
            return false;
        }

        var suffix = Normalize(domainSuffix).TrimStart('@', '.');
        var host = address.Host;
        return host == suffix || host.EndsWith("." + suffix, StringComparison.Ordinal);
    }
}
