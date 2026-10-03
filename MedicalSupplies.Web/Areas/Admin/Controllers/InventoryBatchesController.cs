using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MedicalSupplies.Infrastructure.Identity;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,Admin,InventoryManager")]
public class InventoryBatchesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InventoryBatchesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<string?> GetCurrentUserDisplayNameAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return User.Identity?.Name;

        return string.IsNullOrWhiteSpace(user.FullName)
            ? user.UserName
            : user.FullName;
    }

    public async Task<IActionResult> Index(int? productId)
    {
        var query = _context.InventoryBatches
            .Include(b => b.Product)
            .Include(b => b.Supplier)
            .Where(b => b.Status == BatchStatus.Active)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(b => b.ProductId == productId.Value);
        }

        var batches = await query
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        ViewBag.ProductId = productId;
        return View(batches);
    }

    public async Task<IActionResult> Create(int? productId)
    {
        var vm = new InventoryBatchFormViewModel
        {
            ProductId = productId ?? 0,
            ProductOptions = await GetProductOptionsAsync(),
            SupplierOptions = await GetSupplierOptionsAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InventoryBatchFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.ProductOptions = await GetProductOptionsAsync();
            vm.SupplierOptions = await GetSupplierOptionsAsync();
            return View(vm);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

        if (vm.ManufacturingDate.HasValue && vm.ManufacturingDate.Value > today)
        {
            ModelState.AddModelError(
                nameof(vm.ManufacturingDate),
                $"Manufacturing date cannot be later than today ({today:dd MMM yyyy}).");

            vm.ProductOptions = await GetProductOptionsAsync();
            vm.SupplierOptions = await GetSupplierOptionsAsync();
            return View(vm);
        }

        if (vm.ExpiryDate.HasValue && vm.ExpiryDate.Value < today)
        {
            ModelState.AddModelError(
                nameof(vm.ExpiryDate),
                $"Expiry date cannot be earlier than today ({today:dd MMM yyyy}).");

            vm.ProductOptions = await GetProductOptionsAsync();
            vm.SupplierOptions = await GetSupplierOptionsAsync();
            return View(vm);
        }

        if (vm.ManufacturingDate.HasValue &&
            vm.ExpiryDate.HasValue &&
            vm.ExpiryDate.Value < vm.ManufacturingDate.Value)
        {
            ModelState.AddModelError(
                nameof(vm.ExpiryDate),
                "Expiry date cannot be earlier than the manufacturing date.");

            vm.ProductOptions = await GetProductOptionsAsync();
            vm.SupplierOptions = await GetSupplierOptionsAsync();
            return View(vm);
        }

        var changedBy = await GetCurrentUserDisplayNameAsync();

        var batch = new InventoryBatch
        {
            ProductId = vm.ProductId,
            SupplierId = vm.SupplierId,
            BatchNumber = vm.BatchNumber,
            ManufacturingDate = vm.ManufacturingDate,
            ExpiryDate = vm.ExpiryDate,
            QuantityReceived = vm.QuantityReceived,
            QuantityAvailable = vm.QuantityReceived,
            PurchasePrice = vm.PurchasePrice,
            Status = BatchStatus.Active
        };
        _context.InventoryBatches.Add(batch);

        _context.StockMovements.Add(new StockMovement
        {
            ProductId = vm.ProductId,
            Batch = batch,
            MovementType = StockMovementType.Purchase,
            QuantityChange = vm.QuantityReceived,
            Notes = "Batch received into stock.",
            CreatedBy = changedBy
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Batch recorded and stock updated.";
        return RedirectToAction(nameof(Index), new { productId = vm.ProductId });
    }

    private async Task<List<SelectListItemModel>> GetProductOptionsAsync() =>
        await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProductName)
            .Select(p => new SelectListItemModel { Value = p.ProductId, Text = p.ProductName + " (" + p.ProductCode + ")" })
            .ToListAsync();

    private async Task<List<SelectListItemModel>> GetSupplierOptionsAsync() =>
        await _context.Suppliers
            .Where(s => s.IsActive)
            .OrderBy(s => s.SupplierName)
            .Select(s => new SelectListItemModel { Value = s.SupplierId, Text = s.SupplierName })
            .ToListAsync();
}
