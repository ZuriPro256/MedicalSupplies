namespace MedicalSupplies.Web.ViewModels.Admin;

public class SupplierDetailsViewModel
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    // "Which batches/products did we purchase from this supplier?"
    public List<SupplierProductSummaryViewModel> SuppliedProducts { get; set; } = new();
    public List<SupplierBatchViewModel> InventoryBatches { get; set; } = new();
}

public class SupplierProductSummaryViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int BatchCount { get; set; }
    public int TotalQuantityAvailable { get; set; }
}

public class SupplierBatchViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public int QuantityReceived { get; set; }
    public int QuantityAvailable { get; set; }
    public DateTime ReceivedDate { get; set; }
}
