using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

public class PurchaseOrderStatusHistory
{
    public int PurchaseOrderStatusHistoryId { get; set; }

    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public PurchaseOrderStatus Status { get; set; }

    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;

    /// <summary>AspNetUsers.Id or display name of the staff member who made the change.</summary>
    public string? ChangedBy { get; set; }

    /// <summary>Optional explanation, such as a cancellation reason.</summary>
    public string? Notes { get; set; }
}
