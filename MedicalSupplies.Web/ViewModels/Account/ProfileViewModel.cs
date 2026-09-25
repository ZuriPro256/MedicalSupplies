using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Account;

public class ProfileViewModel
{
    [Required, StringLength(100), Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [StringLength(100), Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [StringLength(200), Display(Name = "Organization / Company")]
    public string? OrganizationName { get; set; }

    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string? Phone { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }
}
