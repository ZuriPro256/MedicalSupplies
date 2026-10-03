using System.ComponentModel.DataAnnotations;
using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class QuotationPricingViewModel
{
    public int QuotationId { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public QuotationStatus Status { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime RequestDate { get; set; }
    public string? CustomerNotes { get; set; }

    public List<QuotationLinePricingViewModel> Lines { get; set; } = new();

    [Display(Name = "Delivery Location")]
    [StringLength(300)]
    public string? DeliveryLocation { get; set; }

    [Display(Name = "Discount (UGX)")]
    [Range(0, double.MaxValue)]
    public decimal? DiscountAmount { get; set; }

    [Display(Name = "Valid Until")]
    [DataType(DataType.Date)]
    public DateOnly? ValidUntil { get; set; }

    [Display(Name = "Internal Notes")]
    [StringLength(1000)]
    public string? AdminNotes { get; set; }

    // Customer rejection feedback
    public QuotationRejectionReason? RejectionReason { get; set; }
    public decimal? CustomerExpectedPrice { get; set; }
    public string? CustomerRejectionComment { get; set; }
    public DateTime? RejectedDate { get; set; }

    public List<QuotationOfferSummaryViewModel> Offers { get; set; } = new();

    public QuotationAcceptedOfferViewModel? AcceptedOffer { get; set; }

    // Computed for display — not posted back.
    public decimal SubTotal => Lines.Sum(l => l.LineTotal);
    public decimal GrandTotal => Math.Max(0, SubTotal - (DiscountAmount ?? 0));
}

public class QuotationLinePricingViewModel
{
    public int QuotationDetailId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? PackSize { get; set; }
    public int Quantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
    public decimal? UnitPrice { get; set; }

    public decimal LineTotal => (UnitPrice ?? 0) * Quantity;
}


public class QuotationAcceptedOfferViewModel
{
    public int QuotationOfferId { get; set; }
    public string OfferNumber { get; set; } = string.Empty;
    public int RevisionNumber { get; set; }
    public List<QuotationAcceptedOfferLineViewModel> Lines { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DeliveryCost { get; set; }
    public decimal GrandTotal { get; set; }
}

public class QuotationAcceptedOfferLineViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public string? PackSize { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class QuotationOfferSummaryViewModel
{
    public int QuotationOfferId { get; set; }
    public string OfferNumber { get; set; } = string.Empty;
    public int RevisionNumber { get; set; }
    public QuotationOfferStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? SentDate { get; set; }
    public decimal? TotalAmount { get; set; }
}
