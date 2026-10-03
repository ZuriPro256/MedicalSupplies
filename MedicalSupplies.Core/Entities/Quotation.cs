using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

public class Quotation
{
    public int QuotationId { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public string? DeliveryLocation { get; set; }
    public string? CustomerNotes { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Pending;

    /// <summary>AspNetUsers.Id of the staff member who prepared this quotation.</summary>
    public string? PreparedBy { get; set; }
    public DateTime? PreparedDate { get; set; }

    /// <summary>Flat discount applied to the priced subtotal; null if none.</summary>
    public decimal? DiscountAmount { get; set; }

    /// <summary>Grand total after discount (sum of QuotationDetail totals, minus DiscountAmount).</summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>Internal notes from the admin/sales team — not shown to the customer.</summary>
    public string? AdminNotes { get; set; }

    public DateOnly? ValidUntil { get; set; }

    /// <summary>Reason selected by the customer when rejecting the quotation.</summary>
    public QuotationRejectionReason? RejectionReason { get; set; }

    /// <summary>Optional price the customer would prefer to pay.</summary>
    public decimal? CustomerExpectedPrice { get; set; }

    /// <summary>Optional explanation or negotiation comment from the customer.</summary>
    public string? CustomerRejectionComment { get; set; }

    /// <summary>When the customer rejected the quotation.</summary>
    public DateTime? RejectedDate { get; set; }

    public ICollection<QuotationDetail> Details { get; set; } = new List<QuotationDetail>();
    public ICollection<QuotationOffer> Offers { get; set; } = new List<QuotationOffer>();

    public int? AcceptedOfferId { get; set; }
    public QuotationOffer? AcceptedOffer { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
