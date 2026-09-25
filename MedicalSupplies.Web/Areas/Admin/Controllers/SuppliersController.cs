using MedicalSupplies.Core.Entities;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,Admin,InventoryManager")]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? search, bool? isActive)
    {
        var query = _context.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.SupplierName.Contains(search) || s.SupplierCode.Contains(search));
        }
        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var suppliers = await query
            .OrderBy(s => s.SupplierName)
            .Select(s => new SupplierListItemViewModel
            {
                SupplierId = s.SupplierId,
                SupplierCode = s.SupplierCode,
                SupplierName = s.SupplierName,
                Country = s.Country,
                Phone = s.Phone,
                Email = s.Email,
                IsActive = s.IsActive
            })
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.IsActive = isActive;
        return View(suppliers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null) return NotFound();

        // "Which batches/products did we purchase from this supplier?"
        var batches = await _context.InventoryBatches
            .Include(b => b.Product)
            .Where(b => b.SupplierId == id)
            .OrderByDescending(b => b.ReceivedDate)
            .ToListAsync();

        var suppliedProducts = batches
            .GroupBy(b => b.Product.ProductName)
            .Select(g => new SupplierProductSummaryViewModel
            {
                ProductName = g.Key,
                BatchCount = g.Count(),
                TotalQuantityAvailable = g.Sum(b => b.QuantityAvailable)
            })
            .OrderBy(p => p.ProductName)
            .ToList();

        var inventoryBatches = batches.Select(b => new SupplierBatchViewModel
        {
            ProductName = b.Product.ProductName,
            BatchNumber = b.BatchNumber,
            ExpiryDate = b.ExpiryDate,
            QuantityReceived = b.QuantityReceived,
            QuantityAvailable = b.QuantityAvailable,
            ReceivedDate = b.ReceivedDate
        }).ToList();

        var vm = new SupplierDetailsViewModel
        {
            SupplierId = supplier.SupplierId,
            SupplierCode = supplier.SupplierCode,
            SupplierName = supplier.SupplierName,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            Country = supplier.Country,
            TaxNumber = supplier.TaxNumber,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive,
            CreatedDate = supplier.CreatedDate,
            SuppliedProducts = suppliedProducts,
            InventoryBatches = inventoryBatches
        };
        return View(vm);
    }

    public IActionResult Create() => View(new SupplierFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierFormViewModel vm)
    {
        if (await _context.Suppliers.AnyAsync(s => s.SupplierCode == vm.SupplierCode))
        {
            ModelState.AddModelError(nameof(vm.SupplierCode), "A supplier with this code already exists.");
        }

        if (!ModelState.IsValid) return View(vm);

        _context.Suppliers.Add(new Supplier
        {
            SupplierCode = vm.SupplierCode,
            SupplierName = vm.SupplierName,
            ContactPerson = vm.ContactPerson,
            Phone = vm.Phone,
            Email = vm.Email,
            Address = vm.Address,
            Country = vm.Country,
            TaxNumber = vm.TaxNumber,
            Notes = vm.Notes,
            IsActive = vm.IsActive
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Supplier created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null) return NotFound();

        return View(new SupplierFormViewModel
        {
            SupplierId = supplier.SupplierId,
            SupplierCode = supplier.SupplierCode,
            SupplierName = supplier.SupplierName,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            Country = supplier.Country,
            TaxNumber = supplier.TaxNumber,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplierFormViewModel vm)
    {
        if (id != vm.SupplierId) return NotFound();

        if (await _context.Suppliers.AnyAsync(s => s.SupplierCode == vm.SupplierCode && s.SupplierId != id))
        {
            ModelState.AddModelError(nameof(vm.SupplierCode), "A supplier with this code already exists.");
        }

        if (!ModelState.IsValid) return View(vm);

        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null) return NotFound();

        supplier.SupplierCode = vm.SupplierCode;
        supplier.SupplierName = vm.SupplierName;
        supplier.ContactPerson = vm.ContactPerson;
        supplier.Phone = vm.Phone;
        supplier.Email = vm.Email;
        supplier.Address = vm.Address;
        supplier.Country = vm.Country;
        supplier.TaxNumber = vm.TaxNumber;
        supplier.Notes = vm.Notes;
        supplier.IsActive = vm.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Supplier updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null) return NotFound();

        // Batches/purchase orders already reference this supplier, so we
        // deactivate rather than delete — same pattern as Categories/Brands.
        supplier.IsActive = false;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Supplier deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null) return NotFound();

        supplier.IsActive = true;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Supplier reactivated.";
        return RedirectToAction(nameof(Index));
    }
}
