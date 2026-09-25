using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Core.Entities;

public class Customer
{
    public int CustomerId { get; set; }

    /// <summary>AspNetUsers.Id, set once the customer has a login.</summary>
    public string? UserId { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? OrganizationName { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Enquiry> Enquiries { get; set; } = new List<Enquiry>();
}
