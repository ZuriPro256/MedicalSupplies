namespace MedicalSupplies.Core.Enums;

public enum StockMovementType
{
    /// <summary>Ad-hoc batch entered directly on a product (no PO behind it).</summary>
    Purchase,

    /// <summary>Goods received against a PurchaseOrder — the formal procurement path.</summary>
    PurchaseReceipt,

    /// <summary>Stock deducted when an order moves to Processing (FEFO allocation).</summary>
    OrderProcessing,

    /// <summary>Stock restored when a Processing order is Cancelled — always a new
    /// movement referencing the OrderProcessing movement it reverses, never an
    /// edit to that original movement.</summary>
    OrderCancellation,

    /// <summary>Manual correction (stock count adjustment, etc.).</summary>
    Adjustment,

    /// <summary>Physical return of already-delivered goods (distinct from an
    /// OrderCancellation, which happens before dispatch).</summary>
    Return,

    Damage,
    Expiry
}
