using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class ReceiveLineViewModel
{
    public int PurchaseOrderDetailId { get; set; }
    public int PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityRemaining => Quantity - QuantityReceived;

    public List<ReceiveBatchRowViewModel> Batches { get; set; } = new() { new(), new() };
}

public class ReceiveBatchRowViewModel
{
    [StringLength(100)]
    public string? BatchNumber { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? ExpiryDate { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}
