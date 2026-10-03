namespace MedicalSupplies.Core.Entities;

public class AccountRecoveryRequest
{
    public int AccountRecoveryRequestId { get; set; }

    public int CustomerId { get; set; }

    public string RecoveryType { get; set; } = "RegisteredContact";

    public string? SubmittedEmail { get; set; }

    public string? SubmittedPhone { get; set; }

    public string? NewContactPhone { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = "Pending";

    public string? VerificationMethod { get; set; }

    public string? VerificationNotes { get; set; }

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
}
