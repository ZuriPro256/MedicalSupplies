using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

public class Enquiry
{
    public int EnquiryId { get; set; }

    /// <summary>Null when submitted by an unregistered website visitor.</summary>
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public EnquiryStatus Status { get; set; } = EnquiryStatus.New;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
