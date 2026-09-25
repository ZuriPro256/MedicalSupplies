using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class PurchaseOrderFormViewModel
{
    [Required, Display(Name = "Supplier")]
    public int SupplierId { get; set; }

    [DataType(DataType.Date), Display(Name = "Expected Delivery Date")]
    public DateOnly? ExpectedDeliveryDate { get; set; }

    [Display(Name = "Delivery Cost")]
    [Range(0, double.MaxValue)]
    public decimal? DeliveryCost { get; set; }

    [Display(Name = "Tax / VAT")]
    [Range(0, double.MaxValue)]
    public decimal? TaxAmount { get; set; }

    [Required, StringLength(10)]
    public string Currency { get; set; } = "UGX";

    [StringLength(1000)]
    public string? Notes { get; set; }

    public List<PurchaseOrderLineFormViewModel> Lines { get; set; } = new();

    public List<SelectListItemModel> SupplierOptions { get; set; } = new();
    public List<SelectListItemModel> ProductOptions { get; set; } = new();
}

public class PurchaseOrderLineFormViewModel
{
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitCost { get; set; }
}
