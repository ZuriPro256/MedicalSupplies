using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class ProductListItemViewModel
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? BrandName { get; set; }

    public decimal? SellingPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public DateOnly? SaleStartDate { get; set; }
    public DateOnly? SaleEndDate { get; set; }

    public ProductAvailability Availability { get; set; }

    public int TotalAvailable { get; set; }
    public int ReorderLevel { get; set; }
    public bool RequiresBatchTracking { get; set; }
    public bool IsActive { get; set; }

    public bool IsLowStock =>
        RequiresBatchTracking &&
        TotalAvailable <= ReorderLevel;

    public bool IsOnSale
    {
        get
        {
            if (!SellingPrice.HasValue ||
                !SalePrice.HasValue ||
                SalePrice.Value >= SellingPrice.Value)
            {
                return false;
            }

            var today = DateOnly.FromDateTime(
                DateTime.UtcNow.AddHours(3));

            if (SaleStartDate.HasValue &&
                today < SaleStartDate.Value)
            {
                return false;
            }

            if (SaleEndDate.HasValue &&
                today > SaleEndDate.Value)
            {
                return false;
            }

            return true;
        }
    }

    public ProductAvailability EffectiveAvailability
    {
        get
        {
            if (Availability != ProductAvailability.InStock)
                return Availability;

            if (!RequiresBatchTracking)
                return ProductAvailability.InStock;

            return TotalAvailable > 0
                ? ProductAvailability.InStock
                : ProductAvailability.OutOfStock;
        }
    }
}
