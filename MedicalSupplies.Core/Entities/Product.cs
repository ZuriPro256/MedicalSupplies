namespace MedicalSupplies.Core.Entities;

public class Product
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public string? Description { get; set; }
    public string? Specifications { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? PackSize { get; set; }
    public decimal? SellingPrice { get; set; }
    public int ReorderLevel { get; set; }

    /// <summary>
    /// False for durable equipment (e.g. a wheelchair) that doesn't need
    /// batch/expiry tracking; true for consumables and pharmaceuticals.
    /// </summary>
    public bool RequiresBatchTracking { get; set; } = true;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<InventoryBatch> Batches { get; set; } = new List<InventoryBatch>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
