namespace MedicalSupplies.Core.Entities;

public class PurchaseOrderDetail
{
    public int PurchaseOrderDetailId { get; set; }

    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    /// <summary>Running total received across every ReceiveLine session for
    /// this line — supports partial deliveries spread over time.</summary>
    public int QuantityReceived { get; set; }

    public int QuantityRemaining => Quantity - QuantityReceived;
}
