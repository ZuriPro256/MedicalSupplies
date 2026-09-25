using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class PurchaseOrderDetailsViewModel
{
    public int PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; }

    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierContact { get; set; }
    public DateTime OrderDate { get; set; }
    public DateOnly? ExpectedDeliveryDate { get; set; }

    public List<PurchaseOrderLineViewModel> Lines { get; set; } = new();

    public decimal Subtotal { get; set; }
    public decimal? DeliveryCost { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "UGX";
    public PaymentStatus PaymentStatus { get; set; }

    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public List<PurchaseOrderStatus> AvailableNextStatuses { get; set; } = new();
    public bool CanReceive { get; set; }
}

public class PurchaseOrderLineViewModel
{
    public int PurchaseOrderDetailId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityRemaining => Quantity - QuantityReceived;
}
