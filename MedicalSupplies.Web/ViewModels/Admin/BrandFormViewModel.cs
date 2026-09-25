using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class BrandFormViewModel
{
    public int BrandId { get; set; }

    [Required, StringLength(150)]
    public string BrandName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
