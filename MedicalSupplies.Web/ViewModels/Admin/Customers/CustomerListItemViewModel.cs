namespace MedicalSupplies.Web.ViewModels.Admin.Customers;

public class CustomerListItemViewModel
{
    public int CustomerId { get; set; }
    public string? OrganizationName { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string CustomerType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
