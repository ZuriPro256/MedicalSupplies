namespace MedicalSupplies.Web.ViewModels.Admin.AccountRecovery;

public class AccountRecoveryDetailsViewModel
{
    public int AccountRecoveryRequestId { get; set; }

    public string RecoveryType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? SubmittedEmail { get; set; }

    public string? SubmittedPhone { get; set; }

    public string? NewContactCountryCode { get; set; }

    public string? NewContactPhone { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public string? VerificationMethod { get; set; }

    public string? VerificationNotes { get; set; }

    public CustomerRecoveryDetailsViewModel Customer { get; set; } = new();

    public List<CustomerRecoveryQuotationViewModel> Quotations { get; set; } = new();

    public List<CustomerRecoveryOrderViewModel> Orders { get; set; } = new();
}

public class CustomerRecoveryDetailsViewModel
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
}

public class CustomerRecoveryQuotationViewModel
{
    public int QuotationId { get; set; }

    public string QuotationNumber { get; set; } = string.Empty;

    public DateTime RequestDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal? TotalAmount { get; set; }
}

public class CustomerRecoveryOrderViewModel
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
}
