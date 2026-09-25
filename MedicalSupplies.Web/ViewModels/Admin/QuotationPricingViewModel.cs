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
