namespace MedicalSupplies.Core.Entities;

public class SupplierPayment
{
    public int SupplierPaymentId { get; set; }

    public int PurchaseOrderId { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public string PaymentMethod { get; set; } = "Bank Transfer";

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    /// <summary>AspNetUsers.Id / display name of the staff member who recorded the payment.</summary>
    public string? RecordedBy { get; set; }

    /// <summary>Whether this payment has been reversed.</summary>
    public bool IsReversed { get; set; }

    /// <summary>UTC date/time when the payment was reversed.</summary>
    public DateTime? ReversedDate { get; set; }

    /// <summary>Display name of the SuperAdmin who reversed the payment.</summary>
    public string? ReversedBy { get; set; }

    /// <summary>Mandatory explanation for why the payment was reversed.</summary>
    public string? ReversalReason { get; set; }
}
