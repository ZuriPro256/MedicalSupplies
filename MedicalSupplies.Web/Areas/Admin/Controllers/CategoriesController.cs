using MedicalSupplies.Core.Entities;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();
        return View(categories);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new CategoryFormViewModel { ParentCategoryOptions = await GetParentOptionsAsync(null) };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.ParentCategoryOptions = await GetParentOptionsAsync(null);
            return View(vm);
        }

        _context.Categories.Add(new Category
        {
            CategoryName = vm.CategoryName,
            ParentCategoryId = vm.ParentCategoryId,
            Description = vm.Description,
            IsActive = vm.IsActive
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Category created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null) return NotFound();

        var vm = new CategoryFormViewModel
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ParentCategoryId = category.ParentCategoryId,
            Description = category.Description,
            IsActive = category.IsActive,
            ParentCategoryOptions = await GetParentOptionsAsync(category.CategoryId)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel vm)
    {
        if (id != vm.CategoryId) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.ParentCategoryOptions = await GetParentOptionsAsync(id);
            return View(vm);
        }

        var category = await _context.Categories.FindAsync(id);
        if (category is null) return NotFound();

        category.CategoryName = vm.CategoryName;
        category.ParentCategoryId = vm.ParentCategoryId == id ? null : vm.ParentCategoryId;
        category.Description = vm.Description;
        category.IsActive = vm.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Category updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null) return NotFound();

        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            TempData["Error"] = "Can't delete a category that still has products. Deactivate it instead.";
            return RedirectToAction(nameof(Index));
        }

        var hasChildren = await _context.Categories.AnyAsync(c => c.ParentCategoryId == id);
        if (hasChildren)
        {
            TempData["Error"] = "Can't delete a category that still has subcategories. Move or delete the subcategories first.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItemModel>> GetParentOptionsAsync(int? excludeId)
    {
        var query = _context.Categories.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.CategoryId != excludeId.Value);
        }

        return await query
            .OrderBy(c => c.CategoryName)
            .Select(c => new SelectListItemModel { Value = c.CategoryId, Text = c.CategoryName })
            .ToListAsync();
    }
}
