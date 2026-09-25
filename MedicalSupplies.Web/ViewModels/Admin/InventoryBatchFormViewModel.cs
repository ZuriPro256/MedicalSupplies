using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class InventoryBatchFormViewModel
{
    public int BatchId { get; set; }

    [Required, Display(Name = "Product")]
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "Supplier")]
    public int? SupplierId { get; set; }

    [StringLength(100), Display(Name = "Batch Number")]
    public string? BatchNumber { get; set; }

    [DataType(DataType.Date), Display(Name = "Manufacturing Date")]
    public DateOnly? ManufacturingDate { get; set; }

    [DataType(DataType.Date), Display(Name = "Expiry Date")]
    public DateOnly? ExpiryDate { get; set; }

    [Required, Range(1, int.MaxValue), Display(Name = "Quantity Received")]
    public int QuantityReceived { get; set; }

    [Display(Name = "Purchase Price (UGX)")]
    [Range(0, double.MaxValue)]
    public decimal? PurchasePrice { get; set; }

    public List<SelectListItemModel> ProductOptions { get; set; } = new();
    public List<SelectListItemModel> SupplierOptions { get; set; } = new();
}
