using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Web.ViewModels.Account;

public class MyQuotationListItemViewModel
{
    public int QuotationId { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public QuotationStatus Status { get; set; }
}

public class MyQuotationDetailsViewModel
{
    public int QuotationId { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public QuotationStatus Status { get; set; }
    public string? DeliveryLocation { get; set; }
    public DateOnly? ValidUntil { get; set; }

    public List<MyQuotationLineViewModel> Lines { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TotalAmount { get; set; }

    // Only shown once the quotation has actually been converted.
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
}

public class MyQuotationLineViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    // Null while still Pending — nothing's been priced yet.
    public decimal? UnitPrice { get; set; }
    public decimal? LineTotal { get; set; }
}
