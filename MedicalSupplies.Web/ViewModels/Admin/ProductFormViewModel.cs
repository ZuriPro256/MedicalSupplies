using System.ComponentModel.DataAnnotations;
using MedicalSupplies.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class ProductFormViewModel
{
    public int ProductId { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;

    [Required, StringLength(250)]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Required, Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "Brand")]
    public int? BrandId { get; set; }

    [StringLength(150), Display(Name = "Brand Name")]
    public string? BrandName { get; set; }

    [StringLength(100), Display(Name = "Brand Country of Origin")]
    public string? BrandCountry { get; set; }

    [StringLength(100), Display(Name = "Country of Origin")]
    public string? CountryOfOrigin { get; set; }

    public string? Description { get; set; }

    public string? Specifications { get; set; }

    [StringLength(50), Display(Name = "Unit of Measure")]
    public string? UnitOfMeasure { get; set; }

    [StringLength(100), Display(Name = "Pack Size")]
    public string? PackSize { get; set; }

    [Display(Name = "Regular Selling Price (UGX)")]
    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
    public decimal? SellingPrice { get; set; }

    [Display(Name = "Sale Price (UGX)")]
    [Range(0, double.MaxValue, ErrorMessage = "Sale price cannot be negative.")]
    public decimal? SalePrice { get; set; }

    [Display(Name = "Sale Start Date")]
    [DataType(DataType.Date)]
    public DateOnly? SaleStartDate { get; set; }

    [Display(Name = "Sale End Date")]
    [DataType(DataType.Date)]
    public DateOnly? SaleEndDate { get; set; }

    [StringLength(500)]
    [Display(Name = "Price Change Reason")]
    public string? PriceChangeReason { get; set; }

    [Required, Display(Name = "Catalogue Availability")]
    public ProductAvailability Availability { get; set; } = ProductAvailability.InStock;

    [Display(Name = "Reorder Level")]
    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; }

    [Display(Name = "Requires batch/expiry tracking")]
    public bool RequiresBatchTracking { get; set; } = true;

    public bool IsActive { get; set; } = true;

    // New images to upload on this save (in addition to any already saved).
    [Display(Name = "Add Images")]
    public List<IFormFile>? NewImages { get; set; }

    public List<ProductImageViewModel> ExistingImages { get; set; } = new();

    public List<SelectListItemModel> CategoryOptions { get; set; } = new();
    public List<SelectListItemModel> BrandOptions { get; set; } = new();
}

public class ProductImageViewModel
{
    public int ProductImageId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
