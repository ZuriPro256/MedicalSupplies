namespace MedicalSupplies.Core.Entities;

public class QuotationOfferDetail
{
    public int QuotationOfferDetailId { get; set; }

    public int QuotationOfferId { get; set; }
    public QuotationOffer QuotationOffer { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }
}
