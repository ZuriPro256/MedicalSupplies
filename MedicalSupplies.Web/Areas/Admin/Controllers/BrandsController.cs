using MedicalSupplies.Core.Entities;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class BrandsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BrandsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var brands = await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();
        return View(brands);
    }

    public IActionResult Create() => View(new BrandFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BrandFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        _context.Brands.Add(new Brand
        {
            BrandName = vm.BrandName,
            Country = vm.Country,
            Description = vm.Description,
            IsActive = vm.IsActive
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Brand created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand is null) return NotFound();

        return View(new BrandFormViewModel
        {
            BrandId = brand.BrandId,
            BrandName = brand.BrandName,
            Country = brand.Country,
            Description = brand.Description,
            IsActive = brand.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BrandFormViewModel vm)
    {
        if (id != vm.BrandId) return NotFound();
        if (!ModelState.IsValid) return View(vm);

        var brand = await _context.Brands.FindAsync(id);
        if (brand is null) return NotFound();

        brand.BrandName = vm.BrandName;
        brand.Country = vm.Country;
        brand.Description = vm.Description;
        brand.IsActive = vm.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Brand updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _context.Brands.FindAsync(id);
        if (brand is null) return NotFound();

        var hasProducts = await _context.Products.AnyAsync(p => p.BrandId == id);
        if (hasProducts)
        {
            TempData["Error"] = "Can't delete a brand that still has products. Deactivate it instead.";
            return RedirectToAction(nameof(Index));
        }

        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Brand deleted.";
        return RedirectToAction(nameof(Index));
    }
}
