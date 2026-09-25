using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

public class PurchaseOrder
{
    public int PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;

    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateOnly? ExpectedDeliveryDate { get; set; }

    /// <summary>Draft -> Submitted -> Approved -> PartiallyReceived -> Received,
    /// cancellable before Received. Never set to Received/PartiallyReceived by
    /// hand — only by actually receiving goods (see ReceiveLine).</summary>
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    public decimal Subtotal { get; set; }
    public decimal? DeliveryCost { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "UGX";

    /// <summary>Whether *we* have paid the supplier for this PO — independent
    /// of receiving status, same relationship as Order.PaymentStatus.</summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public string? Notes { get; set; }

    /// <summary>AspNetUsers.Id / display name of the staff member who raised this purchase order.</summary>
    public string? CreatedBy { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public ICollection<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
    public ICollection<InventoryBatch> Batches { get; set; } = new List<InventoryBatch>();
}
