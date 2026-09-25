using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

public class InventoryBatch
{
    public int BatchId { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    public string? BatchNumber { get; set; }
    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityAvailable { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public BatchStatus Status { get; set; } = BatchStatus.Active;

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
