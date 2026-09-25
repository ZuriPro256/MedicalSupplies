using System.ComponentModel.DataAnnotations;

namespace MedicalSupplies.Web.ViewModels.Admin;

public class CategoryFormViewModel
{
    public int CategoryId { get; set; }

    [Required, StringLength(150)]
    public string CategoryName { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SelectListItemModel> ParentCategoryOptions { get; set; } = new();
}

public class SelectListItemModel
{
    public int Value { get; set; }
    public string Text { get; set; } = string.Empty;
}
