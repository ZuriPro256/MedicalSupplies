namespace MedicalSupplies.Core.Entities;

public class QuotationDetail
{
    public int QuotationDetailId { get; set; }

    public int QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
}
