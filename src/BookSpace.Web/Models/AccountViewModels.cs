using System.ComponentModel.DataAnnotations;

namespace BookSpace.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "შეიყვანეთ ელფოსტა.")]
    [Display(Name = "ელფოსტა")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "შეიყვანეთ პაროლი.")]
    [DataType(DataType.Password)]
    [Display(Name = "პაროლი")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "შეიყვანეთ ელფოსტა.")]
    [Display(Name = "უნივერსიტეტის ელფოსტა")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "შეიყვანეთ პაროლი.")]
    [DataType(DataType.Password)]
    [Display(Name = "პაროლი")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "გაიმეორეთ პაროლი.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "პაროლები არ ემთხვევა.")]
    [Display(Name = "გაიმეორეთ პაროლი")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
