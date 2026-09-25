namespace MedicalSupplies.Web.ViewModels.Admin;

public class ProductListItemViewModel
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public decimal? SellingPrice { get; set; }
    public int TotalAvailable { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock => TotalAvailable <= ReorderLevel;
}
