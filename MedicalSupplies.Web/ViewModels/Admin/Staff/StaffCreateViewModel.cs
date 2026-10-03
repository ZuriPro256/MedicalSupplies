using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin.Staff;

public class StaffCreateViewModel
{
    [Required]
    [Display(Name = "Full Name")]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = "Sales";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Temporary Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<string> Permissions { get; set; } = new();

    public List<PermissionOptionViewModel> AvailablePermissions { get; set; } = new();
}
