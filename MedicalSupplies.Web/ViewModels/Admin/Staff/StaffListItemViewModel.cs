namespace MedicalSupplies.Web.ViewModels.Admin.Staff;

public class StaffListItemViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public bool EmailConfirmed { get; set; }
}
