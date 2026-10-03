using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

public class QuotationOffer
{
    public int QuotationOfferId { get; set; }

    public int QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;

    /// <summary>Sequential revision number within the quotation, starting at 1.</summary>
    public int RevisionNumber { get; set; }

    public QuotationOfferStatus Status { get; set; } = QuotationOfferStatus.Draft;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? SentDate { get; set; }

    public DateTime? RespondedDate { get; set; }

    public string? PreparedBy { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal DeliveryCost { get; set; }

    public string? AdminNotes { get; set; }

    public DateOnly? ValidUntil { get; set; }

    public QuotationRejectionReason? RejectionReason { get; set; }

    public decimal? CustomerExpectedPrice { get; set; }

    public string? CustomerRejectionComment { get; set; }

    public DateTime? RejectedDate { get; set; }

    public string OfferNumber { get; set; } = string.Empty;

    public ICollection<QuotationOfferDetail> Details { get; set; } = new List<QuotationOfferDetail>();
}
