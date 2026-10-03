using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin.AccountRecovery;

public class AccountRecoveryReviewViewModel
{
    public int AccountRecoveryRequestId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? CustomerName { get; set; }

    public string? OrganizationName { get; set; }

    public string? SubmittedEmail { get; set; }

    public string? SubmittedPhone { get; set; }

    [Display(Name = "New Contact Country")]
    public string? NewContactCountryCode { get; set; }

    [Display(Name = "New Contact Phone")]
    [MaxLength(50)]
    public string? NewContactPhone { get; set; }

    public string? Reason { get; set; }

    [Display(Name = "Verification Method")]
    [MaxLength(100)]
    public string? VerificationMethod { get; set; }

    [Display(Name = "Verification Notes")]
    [MaxLength(2000)]
    public string? VerificationNotes { get; set; }

    [Display(Name = "Reviewer Notes")]
    [MaxLength(2000)]
    public string? ReviewerNotes { get; set; }
}
