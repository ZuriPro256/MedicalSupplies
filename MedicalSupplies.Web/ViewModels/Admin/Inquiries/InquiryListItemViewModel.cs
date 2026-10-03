namespace MedicalSupplies.Web.ViewModels.Admin.Inquiries;

using MedicalSupplies.Core.Enums;

public class InquiryListItemViewModel
{
    public int EnquiryId { get; set; }
    public int? CustomerId { get; set; }
    public string? OrganizationName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string MessagePreview { get; set; } = string.Empty;
    public EnquiryStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
}
