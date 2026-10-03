using MedicalSupplies.Core.Enums;

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

    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string? CountryOfOrigin { get; set; }

    public string? Description { get; set; }
    public string? Specifications { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? PackSize { get; set; }

    /// <summary>
    /// Normal catalogue selling price. Actual quotation/order prices remain
    /// stored separately on quotation/order details.
    /// </summary>
    public decimal? SellingPrice { get; set; }

    /// <summary>
    /// Promotional price shown while the sale is active.
    /// </summary>
    public decimal? SalePrice { get; set; }

    /// <summary>
    /// Inclusive start date for the promotional price.
    /// </summary>
    public DateOnly? SaleStartDate { get; set; }

    /// <summary>
    /// Inclusive end date for the promotional price.
    /// </summary>
    public DateOnly? SaleEndDate { get; set; }

    /// <summary>
    /// Customer-facing catalogue availability. This is separate from
    /// inventory/batch tracking because a product can be sourced on request,
    /// require quotation, be coming soon, or be temporarily out of stock.
    /// </summary>
    public ProductAvailability Availability { get; set; } = ProductAvailability.InStock;

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
