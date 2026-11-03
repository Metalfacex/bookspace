namespace BookSpace.Web.Configuration;

/// <summary>
/// REST API-ს rate limiting (appsettings.json → "BookSpace:RateLimit").
/// ერთ მომხმარებელს (ან IP-ს, თუ შესული არ არის) ერთ ფანჯარაში მაქსიმუმ PermitLimit მოთხოვნა.
/// </summary>
public class ApiRateLimitOptions
{
    public const string PolicyName = "api";
    public const string SectionName = "BookSpace:RateLimit";

    public int PermitLimit { get; set; } = 60;
    public int WindowSeconds { get; set; } = 60;
}
