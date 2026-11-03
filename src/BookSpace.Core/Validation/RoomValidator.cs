namespace BookSpace.Core.Validation;

public static class RoomValidator
{
    public const int MinCapacity = 1;
    public const int MaxCapacity = 100;
    public const int MaxNameLength = 100;
    public const int MaxLocationLength = 200;

    /// <summary>აბრუნებს შეცდომების სიას (ცარიელი = ვალიდურია).</summary>
    public static IReadOnlyList<string> Validate(string? name, string? location, int capacity)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("დასახელება სავალდებულოა.");
        }
        else if (name.Trim().Length > MaxNameLength)
        {
            errors.Add($"დასახელება მაქსიმუმ {MaxNameLength} სიმბოლოა.");
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            errors.Add("მდებარეობა სავალდებულოა.");
        }
        else if (location.Trim().Length > MaxLocationLength)
        {
            errors.Add($"მდებარეობა მაქსიმუმ {MaxLocationLength} სიმბოლოა.");
        }

        if (capacity < MinCapacity || capacity > MaxCapacity)
        {
            errors.Add($"ტევადობა უნდა იყოს {MinCapacity}-დან {MaxCapacity}-მდე.");
        }

        return errors;
    }
}
