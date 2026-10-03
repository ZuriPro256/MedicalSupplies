using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Contact;

public class ContactFormViewModel
{
    [Required, StringLength(150)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, StringLength(150)]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [StringLength(50)]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [StringLength(200)]
    [Display(Name = "Subject")]
    public string? Subject { get; set; }

    [Required, StringLength(1000)]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;
}
