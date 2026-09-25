using MedicalSupplies.Core.Entities;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.Services;
using MedicalSupplies.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,Admin,Sales,InventoryManager")]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileUploadService _fileUploadService;

    public ProductsController(ApplicationDbContext context, IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Batches)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.ProductName.Contains(search) || p.ProductCode.Contains(search));
        }

        var products = await query
            .OrderBy(p => p.ProductName)
            .Select(p => new ProductListItemViewModel
            {
                ProductId = p.ProductId,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                CategoryName = p.Category.CategoryName,
                BrandName = p.Brand != null ? p.Brand.BrandName : null,
                SellingPrice = p.SellingPrice,
                ReorderLevel = p.ReorderLevel,
                TotalAvailable = p.Batches
                    .Where(b => b.Status == Core.Enums.BatchStatus.Active)
                    .Sum(b => b.QuantityAvailable),
                IsActive = p.IsActive
            })
            .ToListAsync();

        ViewBag.Search = search;
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new ProductFormViewModel
        {
            CategoryOptions = await GetCategoryOptionsAsync(),
            BrandOptions = await GetBrandOptionsAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        if (await _context.Products.AnyAsync(p => p.ProductCode == vm.ProductCode))
        {
            ModelState.AddModelError(nameof(vm.ProductCode), "A product with this code already exists.");
        }

        if (!ModelState.IsValid)
        {
            vm.CategoryOptions = await GetCategoryOptionsAsync();
            vm.BrandOptions = await GetBrandOptionsAsync();
            return View(vm);
        }

        var product = new Product
        {
            ProductCode = vm.ProductCode,
            ProductName = vm.ProductName,
            CategoryId = vm.CategoryId,
            BrandId = vm.BrandId,
            Description = vm.Description,
            Specifications = vm.Specifications,
            UnitOfMeasure = vm.UnitOfMeasure,
            PackSize = vm.PackSize,
            SellingPrice = vm.SellingPrice,
            ReorderLevel = vm.ReorderLevel,
            RequiresBatchTracking = vm.RequiresBatchTracking,
            IsActive = vm.IsActive
        };

        await SaveNewImagesAsync(product, vm.NewImages);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == id);
        if (product is null) return NotFound();

        var vm = new ProductFormViewModel
        {
            ProductId = product.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            Description = product.Description,
            Specifications = product.Specifications,
            UnitOfMeasure = product.UnitOfMeasure,
            PackSize = product.PackSize,
            SellingPrice = product.SellingPrice,
            ReorderLevel = product.ReorderLevel,
            RequiresBatchTracking = product.RequiresBatchTracking,
            IsActive = product.IsActive,
            ExistingImages = product.Images
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.DisplayOrder)
                .Select(i => new ProductImageViewModel
                {
                    ProductImageId = i.ProductImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                }).ToList(),
            CategoryOptions = await GetCategoryOptionsAsync(),
            BrandOptions = await GetBrandOptionsAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel vm)
    {
        if (id != vm.ProductId) return NotFound();

        if (await _context.Products.AnyAsync(p => p.ProductCode == vm.ProductCode && p.ProductId != id))
        {
            ModelState.AddModelError(nameof(vm.ProductCode), "A product with this code already exists.");
        }

        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == id);
        if (product is null) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.ExistingImages = product.Images
                .Select(i => new ProductImageViewModel
                {
                    ProductImageId = i.ProductImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                }).ToList();
            vm.CategoryOptions = await GetCategoryOptionsAsync();
            vm.BrandOptions = await GetBrandOptionsAsync();
            return View(vm);
        }

        product.ProductCode = vm.ProductCode;
        product.ProductName = vm.ProductName;
        product.CategoryId = vm.CategoryId;
        product.BrandId = vm.BrandId;
        product.Description = vm.Description;
        product.Specifications = vm.Specifications;
        product.UnitOfMeasure = vm.UnitOfMeasure;
        product.PackSize = vm.PackSize;
        product.SellingPrice = vm.SellingPrice;
        product.ReorderLevel = vm.ReorderLevel;
        product.RequiresBatchTracking = vm.RequiresBatchTracking;
        product.IsActive = vm.IsActive;
        product.ModifiedDate = DateTime.UtcNow;

        await SaveNewImagesAsync(product, vm.NewImages);

        await _context.SaveChangesAsync();
        TempData["Success"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int productImageId, int productId)
    {
        var image = await _context.ProductImages.FindAsync(productImageId);
        if (image is not null)
        {
            _fileUploadService.DeleteProductImage(image.ImageUrl);
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryImage(int productImageId, int productId)
    {
        var images = await _context.ProductImages.Where(i => i.ProductId == productId).ToListAsync();
        foreach (var image in images)
        {
            image.IsPrimary = image.ProductImageId == productImageId;
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.IsActive = false;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Product deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SaveNewImagesAsync(Product product, List<IFormFile>? files)
    {
        if (files is null || files.Count == 0) return;

        var hasPrimaryAlready = product.Images.Any(i => i.IsPrimary);
        var nextDisplayOrder = product.Images.Count == 0 ? 0 : product.Images.Max(i => i.DisplayOrder) + 1;

        foreach (var file in files.Where(f => f.Length > 0))
        {
            var url = await _fileUploadService.SaveProductImageAsync(file);
            product.Images.Add(new ProductImage
            {
                ImageUrl = url,
                IsPrimary = !hasPrimaryAlready,
                DisplayOrder = nextDisplayOrder++
            });
            hasPrimaryAlready = true;
        }
    }

    private async Task<List<SelectListItemModel>> GetCategoryOptionsAsync() =>
        await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryName)
            .Select(c => new SelectListItemModel { Value = c.CategoryId, Text = c.CategoryName })
            .ToListAsync();

    private async Task<List<SelectListItemModel>> GetBrandOptionsAsync() =>
        await _context.Brands
            .Where(b => b.IsActive)
            .OrderBy(b => b.BrandName)
            .Select(b => new SelectListItemModel { Value = b.BrandId, Text = b.BrandName })
            .ToListAsync();
}
