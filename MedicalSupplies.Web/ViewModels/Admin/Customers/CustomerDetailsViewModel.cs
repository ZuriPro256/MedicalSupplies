namespace MedicalSupplies.Web.ViewModels.Admin.Customers;

public class CustomerDetailsViewModel
{
    public int CustomerId { get; set; }
    public string? OrganizationName { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string CustomerType { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
    public bool HasLogin { get; set; }
    public List<CustomerQuotationItemViewModel> Quotations { get; set; } = new();
    public List<CustomerOrderItemViewModel> Orders { get; set; } = new();
}

public class CustomerQuotationItemViewModel
{
    public int QuotationId { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? TotalAmount { get; set; }
}

public class CustomerOrderItemViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
