namespace MedicalSupplies.Core.Entities;

public class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // No BatchId here. Which batch(es) fulfilled this line is recorded
    // entirely in StockMovements (OrderDetailId + BatchId + Quantity per
    // batch consumed) — that's the single authoritative record, so this
    // entity never carries a second, potentially-conflicting copy of it.
}
