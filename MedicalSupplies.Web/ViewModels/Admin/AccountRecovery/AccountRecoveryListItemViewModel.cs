namespace MedicalSupplies.Web.ViewModels.Admin.AccountRecovery;

public class AccountRecoveryListItemViewModel
{
    public int AccountRecoveryRequestId { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string? OrganizationName { get; set; }

    public string? SubmittedEmail { get; set; }

    public string? SubmittedPhone { get; set; }

    public string RecoveryType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }
}