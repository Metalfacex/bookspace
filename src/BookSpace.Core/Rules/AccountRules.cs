namespace BookSpace.Core.Rules;

public class AccountRules
{
    /// <summary>რეგისტრაცია დაშვებულია მხოლოდ ამ დომენის (ან მისი ქვედომენის) ელფოსტით.</summary>
    public string AllowedEmailDomain { get; set; } = "edu.ge";
}
