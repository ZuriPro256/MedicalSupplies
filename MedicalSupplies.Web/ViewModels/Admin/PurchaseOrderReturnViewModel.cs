using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class PurchaseOrderReturnViewModel
{
    public int PurchaseOrderId { get; set; }

    public string PurchaseOrderNumber { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    public List<PurchaseOrderReturnBatchViewModel> Batches { get; set; } = new();
}

public class PurchaseOrderReturnBatchViewModel
{
    public int BatchId { get; set; }

    public int PurchaseOrderDetailId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? BatchNumber { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public int QuantityReceived { get; set; }

    public int QuantityAvailable { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityToReturn { get; set; }
}
