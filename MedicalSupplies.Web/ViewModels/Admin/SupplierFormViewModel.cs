using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class SupplierFormViewModel
{
    public int SupplierId { get; set; }

    [Required, StringLength(30), Display(Name = "Supplier Code")]
    public string SupplierCode { get; set; } = string.Empty;

    [Required, StringLength(200), Display(Name = "Supplier Name")]
    public string SupplierName { get; set; } = string.Empty;

    [StringLength(150), Display(Name = "Contact Person")]
    public string? ContactPerson { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(150), EmailAddress]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(50), Display(Name = "Tax / VAT Number")]
    public string? TaxNumber { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
