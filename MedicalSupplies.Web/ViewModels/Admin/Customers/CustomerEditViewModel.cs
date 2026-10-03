using System.ComponentModel.DataAnnotations;
using MedicalSupplies.Core.Enums;

namespace MedicalSupplies.Web.ViewModels.Admin.Customers;

public class CustomerEditViewModel
{
    public int CustomerId { get; set; }

    [Display(Name = "First Name")]
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name")]
    [MaxLength(100)]
    public string? LastName { get; set; }

    [Display(Name = "Organization / Company Name")]
    [MaxLength(200)]
    public string? OrganizationName { get; set; }

    [Display(Name = "Customer Type")]
    public CustomerType CustomerType { get; set; }

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }

    [Required, Display(Name = "Phone Country")]
    public string CountryCode { get; set; } = "UG";

    [MaxLength(50), Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}
