namespace MedicalSupplies.Web.ViewModels.Catalogue;

public class CatalogueListViewModel
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }

    public List<CatalogueProductCardViewModel> Products { get; set; } = new();
    public List<Admin.SelectListItemModel> CategoryOptions { get; set; } = new();
    public List<Admin.SelectListItemModel> BrandOptions { get; set; } = new();

    public int TotalResults { get; set; }
}

public class CatalogueProductCardViewModel
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? PrimaryImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? PackSize { get; set; }
    public bool InStock { get; set; }
}
