using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin.Staff;

public class StaffEditViewModel
{
    public string? UserId { get; set; }

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
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<string> Permissions { get; set; } = new();

    public List<PermissionOptionViewModel> AvailablePermissions { get; set; } = new();
}

public class PermissionOptionViewModel
{
    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Permission { get; set; } = string.Empty;

    public bool Selected { get; set; }
}
