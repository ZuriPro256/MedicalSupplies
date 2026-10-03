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
    public decimal AmountPaid { get; set; }
    public List<SupplierPaymentViewModel> SupplierPayments { get; set; } = new();

    public List<PurchaseOrderStatusHistoryViewModel> StatusHistory { get; set; } = new();

    public List<PurchaseOrderReturnDetailsViewModel> Returns { get; set; } = new();

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

public class SupplierPaymentViewModel
{
    public int SupplierPaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public string? RecordedBy { get; set; }
    public bool IsReversed { get; set; }
    public DateTime? ReversedDate { get; set; }
    public string? ReversedBy { get; set; }
    public string? ReversalReason { get; set; }
}


public class PurchaseOrderStatusHistoryViewModel
{
    public int PurchaseOrderStatusHistoryId { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public DateTime ChangedDate { get; set; }
    public string? ChangedBy { get; set; }
    public string? Notes { get; set; }
}

public class PurchaseOrderReturnDetailsViewModel
{
    public int PurchaseOrderReturnId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? RecordedBy { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseOrderReturnDetailViewModel> Details { get; set; } = new();
}

public class PurchaseOrderReturnDetailViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public int QuantityReturned { get; set; }
    public decimal UnitCost { get; set; }
}
