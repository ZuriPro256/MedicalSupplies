using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Account;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Registered Email")]
    public string Email { get; set; } = string.Empty;

    [Required, Display(Name = "Country")]
    public string CountryCode { get; set; } = "UG";

    [Required, StringLength(50), Display(Name = "Registered Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Reason for password recovery")]
    [StringLength(1000)]
    public string? Reason { get; set; }
}
