using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Catalogue;

public class QuotationCartViewModel
{
    public List<QuotationCartLineViewModel> Lines { get; set; } = new();
}

public class QuotationCartLineViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? PackSize { get; set; }
    public int Quantity { get; set; }
}

public class QuotationRequestFormViewModel
{
    public List<QuotationCartLineViewModel> Lines { get; set; } = new();

    [Required, StringLength(150), Display(Name = "Full Name / Organization Contact")]
    public string ContactName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? OrganizationName { get; set; }

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, Display(Name = "Phone Country")]
    public string CountryCode { get; set; } = "UG";

    [Required, StringLength(50), Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(300), Display(Name = "Delivery Location")]
    public string? DeliveryLocation { get; set; }

    [StringLength(1000), Display(Name = "Notes")]
    public string? CustomerNotes { get; set; }
}
