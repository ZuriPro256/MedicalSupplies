using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required, StringLength(150), Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200), Display(Name = "Organization / Company (optional)")]
    public string? OrganizationName { get; set; }

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, Display(Name = "Country")]
    public string CountryCode { get; set; } = "UG";

    [Required, StringLength(50), Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Address { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "Confirm Password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
