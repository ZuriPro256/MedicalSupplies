using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Web.ViewModels.Catalogue;

public class ProductDetailsViewModel
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? Description { get; set; }
    public string? Specifications { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? PackSize { get; set; }

    public ProductAvailability Availability { get; set; }

    public bool InStock { get; set; }

    public decimal? SellingPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public bool IsOnSale { get; set; }
    public decimal? DisplayPrice => IsOnSale ? SalePrice : SellingPrice;

    public List<string> ImageUrls { get; set; } = new();
    public List<CatalogueProductCardViewModel> RelatedProducts { get; set; } = new();
}
