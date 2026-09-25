using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

/// <summary>
/// The single authoritative record of which batch fulfilled which order
/// line (or was received against which purchase order line). Nothing else
/// in the schema duplicates this — OrderDetail and PurchaseOrderDetail
/// both stay batch-agnostic on purpose.
/// </summary>
public class StockMovement
{
    public int StockMovementId { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? BatchId { get; set; }
    public InventoryBatch? Batch { get; set; }

    public StockMovementType MovementType { get; set; }

    /// <summary>Positive for stock in, negative for stock out.</summary>
    public int QuantityChange { get; set; }

    // Typed links instead of a generic ReferenceType/ReferenceId pair —
    // exactly one of Order/PurchaseOrder is set depending on MovementType
    // (neither is set for a purely manual Adjustment/Purchase entry).
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public int? OrderDetailId { get; set; }
    public OrderDetail? OrderDetail { get; set; }

    public int? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    /// <summary>For a reversal movement (e.g. OrderCancellation), the original
    /// movement it reverses (e.g. the OrderProcessing deduction) — the original
    /// is never edited or deleted, only pointed back to. Null for a movement
    /// that isn't itself a reversal.</summary>
    public int? ReversesStockMovementId { get; set; }
    public StockMovement? ReversesStockMovement { get; set; }

    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    /// <summary>AspNetUsers.Id (or display name) of the staff member who recorded this movement.</summary>
    public string? CreatedBy { get; set; }
}
