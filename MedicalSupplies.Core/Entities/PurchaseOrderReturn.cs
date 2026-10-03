namespace MedicalSupplies.Core.Entities;

public class PurchaseOrderReturn
{
    public int PurchaseOrderReturnId { get; set; }

    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public string ReturnNumber { get; set; } = string.Empty;

    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    public string Reason { get; set; } = string.Empty;

    /// <summary>AspNetUsers.Id or display name of the staff member who recorded the return.</summary>
    public string? RecordedBy { get; set; }

    public string? Notes { get; set; }

    public ICollection<PurchaseOrderReturnDetail> Details { get; set; }
        = new List<PurchaseOrderReturnDetail>();
}

public class PurchaseOrderReturnDetail
{
    public int PurchaseOrderReturnDetailId { get; set; }

    public int PurchaseOrderReturnId { get; set; }
    public PurchaseOrderReturn PurchaseOrderReturn { get; set; } = null!;

    public int PurchaseOrderDetailId { get; set; }
    public PurchaseOrderDetail PurchaseOrderDetail { get; set; } = null!;

    public int BatchId { get; set; }
    public InventoryBatch Batch { get; set; } = null!;

    public int QuantityReturned { get; set; }

    public decimal UnitCost { get; set; }

    public string? Notes { get; set; }
}
