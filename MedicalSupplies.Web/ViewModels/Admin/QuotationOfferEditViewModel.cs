namespace MedicalSupplies.Web.ViewModels.Admin;

public class QuotationOfferEditViewModel
{
    public int QuotationOfferId { get; set; }

    public int QuotationId { get; set; }

    public string OfferNumber { get; set; } = string.Empty;

    public int RevisionNumber { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public decimal? DiscountAmount { get; set; }

    public decimal DeliveryCost { get; set; }

    public DateOnly? ValidUntil { get; set; }

    public string? AdminNotes { get; set; }

    public List<QuotationOfferLineEditViewModel> Lines { get; set; } = new();
}

public class QuotationOfferLineEditViewModel
{
    public int QuotationOfferDetailId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? PackSize { get; set; }

    public int Quantity { get; set; }

    public decimal? UnitPrice { get; set; }
}
