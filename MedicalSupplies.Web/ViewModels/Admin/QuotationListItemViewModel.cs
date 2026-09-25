using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class QuotationListItemViewModel
{
    public int QuotationId { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public int LineCount { get; set; }
    public decimal? TotalAmount { get; set; }
    public QuotationStatus Status { get; set; }
}
