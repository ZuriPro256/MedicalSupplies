using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
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

    public ProductsController(
        ApplicationDbContext context,
        IFileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        int? brandId,
        ProductAvailability? availability,
        string? stock)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Batches)
            .AsQueryable();

        var today = DateOnly.FromDateTime(
            DateTime.UtcNow.AddHours(3));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(p =>
                EF.Functions.ILike(
                    p.ProductName,
                    $"%{searchTerm}%") ||
                EF.Functions.ILike(
                    p.ProductCode,
                    $"%{searchTerm}%"));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p =>
                p.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            query = query.Where(p =>
                p.BrandId == brandId.Value);
        }

        if (availability.HasValue)
        {
            query = query.Where(p =>
                p.Availability == availability.Value);
        }

        switch (stock?.Trim().ToLowerInvariant())
        {
            case "in":
                query = query.Where(p =>
                    (!p.RequiresBatchTracking &&
                     p.Availability == ProductAvailability.InStock) ||
                    (p.RequiresBatchTracking &&
                     p.Batches
                        .Where(b =>
                            b.Status == Core.Enums.BatchStatus.Active &&
                            b.QuantityAvailable > 0 &&
                            (!b.ExpiryDate.HasValue ||
                             b.ExpiryDate.Value >= today))
                        .Sum(b => b.QuantityAvailable) > 0));
                break;

            case "low":
                query = query.Where(p =>
                    p.RequiresBatchTracking &&
                    p.Batches
                        .Where(b =>
                            b.Status == Core.Enums.BatchStatus.Active &&
                            b.QuantityAvailable > 0 &&
                            (!b.ExpiryDate.HasValue ||
                             b.ExpiryDate.Value >= today))
                        .Sum(b => b.QuantityAvailable) > 0 &&
                    p.Batches
                        .Where(b =>
                            b.Status == Core.Enums.BatchStatus.Active &&
                            b.QuantityAvailable > 0 &&
                            (!b.ExpiryDate.HasValue ||
                             b.ExpiryDate.Value >= today))
                        .Sum(b => b.QuantityAvailable) <= p.ReorderLevel);
                break;

            case "out":
                query = query.Where(p =>
                    (!p.RequiresBatchTracking &&
                     p.Availability == ProductAvailability.OutOfStock) ||
                    (p.RequiresBatchTracking &&
                     p.Batches
                        .Where(b =>
                            b.Status == Core.Enums.BatchStatus.Active &&
                            b.QuantityAvailable > 0 &&
                            (!b.ExpiryDate.HasValue ||
                             b.ExpiryDate.Value >= today))
                        .Sum(b => b.QuantityAvailable) == 0));
                break;
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
                SalePrice = p.SalePrice,
                SaleStartDate = p.SaleStartDate,
                SaleEndDate = p.SaleEndDate,
                Availability = p.Availability,
                TotalAvailable = p.Batches
                    .Where(b =>
                        b.Status == Core.Enums.BatchStatus.Active &&
                        b.QuantityAvailable > 0 &&
                        (!b.ExpiryDate.HasValue ||
                         b.ExpiryDate.Value >= today))
                    .Sum(b => b.QuantityAvailable),
                ReorderLevel = p.ReorderLevel,
                RequiresBatchTracking = p.RequiresBatchTracking,
                IsActive = p.IsActive
            })
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.BrandId = brandId;
        ViewBag.Availability = availability;
        ViewBag.Stock = stock;

        ViewBag.Categories = await _context.Categories
            .OrderBy(c => c.CategoryName)
            .Select(c => new SelectListItemModel
            {
                Value = c.CategoryId,
                Text = c.CategoryName
            })
            .ToListAsync();

        ViewBag.Brands = await _context.Brands
            .OrderBy(b => b.BrandName)
            .Select(b => new SelectListItemModel
            {
                Value = b.BrandId,
                Text = b.BrandName
            })
            .ToListAsync();

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
        await ValidateProductFormAsync(vm);

        if (await _context.Products.AnyAsync(
                p => p.ProductCode == vm.ProductCode))
        {
            ModelState.AddModelError(
                nameof(vm.ProductCode),
                "A product with this code already exists.");
        }

        if (!ModelState.IsValid)
        {
            vm.CategoryOptions = await GetCategoryOptionsAsync();
            vm.BrandOptions = await GetBrandOptionsAsync();
            return View(vm);
        }

        var product = new Product
        {
            ProductCode = vm.ProductCode.Trim(),
            ProductName = vm.ProductName.Trim(),
            CategoryId = vm.CategoryId,
            Brand = await ResolveBrandAsync(vm),
            CountryOfOrigin = NullIfWhiteSpace(vm.CountryOfOrigin),
            Description = vm.Description,
            Specifications = vm.Specifications,
            UnitOfMeasure = vm.UnitOfMeasure,
            PackSize = vm.PackSize,
            SellingPrice = vm.SellingPrice,
            SalePrice = vm.SalePrice,
            SaleStartDate = vm.SaleStartDate,
            SaleEndDate = vm.SaleEndDate,
            Availability = vm.Availability,
            ReorderLevel = vm.ReorderLevel,
            RequiresBatchTracking = vm.RequiresBatchTracking,
            IsActive = vm.IsActive
        };

        await SaveNewImagesAsync(product, vm.NewImages);

        _context.Products.Add(product);

        var changedBy = User.Identity?.IsAuthenticated == true
            ? User.Identity?.Name
            : null;

        var priceChangeReason =
            string.IsNullOrWhiteSpace(vm.PriceChangeReason)
                ? "Initial product price"
                : vm.PriceChangeReason.Trim();

        if (vm.SellingPrice.HasValue)
        {
            _context.ProductPriceHistories.Add(
                new ProductPriceHistory
                {
                    Product = product,
                    PriceType = "SellingPrice",
                    PreviousAmount = null,
                    NewAmount = vm.SellingPrice,
                    Currency = "UGX",
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = changedBy,
                    Reason = priceChangeReason
                });
        }

        if (vm.SalePrice.HasValue)
        {
            _context.ProductPriceHistories.Add(
                new ProductPriceHistory
                {
                    Product = product,
                    PriceType = "SalePrice",
                    PreviousAmount = null,
                    NewAmount = vm.SalePrice,
                    Currency = "UGX",
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = changedBy,
                    Reason = priceChangeReason
                });
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product is null)
            return NotFound();

        var vm = new ProductFormViewModel
        {
            ProductId = product.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            BrandName = product.Brand?.BrandName,
            BrandCountry = product.Brand?.Country,
            CountryOfOrigin = product.CountryOfOrigin,
            Description = product.Description,
            Specifications = product.Specifications,
            UnitOfMeasure = product.UnitOfMeasure,
            PackSize = product.PackSize,
            SellingPrice = product.SellingPrice,
            SalePrice = product.SalePrice,
            SaleStartDate = product.SaleStartDate,
            SaleEndDate = product.SaleEndDate,
            Availability = product.Availability,
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
                })
                .ToList(),
            CategoryOptions = await GetCategoryOptionsAsync(),
            BrandOptions = await GetBrandOptionsAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ProductFormViewModel vm)
    {
        if (id != vm.ProductId)
            return NotFound();

        await ValidateProductFormAsync(vm);

        if (await _context.Products.AnyAsync(
                p => p.ProductCode == vm.ProductCode &&
                     p.ProductId != id))
        {
            ModelState.AddModelError(
                nameof(vm.ProductCode),
                "A product with this code already exists.");
        }

        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product is null)
            return NotFound();

        var previousSellingPrice = product.SellingPrice;
        var previousSalePrice = product.SalePrice;

        if (!ModelState.IsValid)
        {
            vm.ExistingImages = product.Images
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.DisplayOrder)
                .Select(i => new ProductImageViewModel
                {
                    ProductImageId = i.ProductImageId,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                })
                .ToList();

            vm.CategoryOptions = await GetCategoryOptionsAsync();
            vm.BrandOptions = await GetBrandOptionsAsync();

            return View(vm);
        }

        product.ProductCode = vm.ProductCode.Trim();
        product.ProductName = vm.ProductName.Trim();
        product.CategoryId = vm.CategoryId;
        product.Brand = await ResolveBrandAsync(vm);
        product.CountryOfOrigin = NullIfWhiteSpace(vm.CountryOfOrigin);
        product.Description = vm.Description;
        product.Specifications = vm.Specifications;
        product.UnitOfMeasure = vm.UnitOfMeasure;
        product.PackSize = vm.PackSize;
        var changedBy = User.Identity?.IsAuthenticated == true
            ? User.Identity?.Name
            : null;

        var priceChangeReason =
            string.IsNullOrWhiteSpace(vm.PriceChangeReason)
                ? "Price updated"
                : vm.PriceChangeReason.Trim();

        if (previousSellingPrice != vm.SellingPrice)
        {
            _context.ProductPriceHistories.Add(
                new ProductPriceHistory
                {
                    ProductId = product.ProductId,
                    PriceType = "SellingPrice",
                    PreviousAmount = previousSellingPrice,
                    NewAmount = vm.SellingPrice,
                    Currency = "UGX",
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = changedBy,
                    Reason = priceChangeReason
                });
        }

        if (previousSalePrice != vm.SalePrice)
        {
            _context.ProductPriceHistories.Add(
                new ProductPriceHistory
                {
                    ProductId = product.ProductId,
                    PriceType = "SalePrice",
                    PreviousAmount = previousSalePrice,
                    NewAmount = vm.SalePrice,
                    Currency = "UGX",
                    ChangedDate = DateTime.UtcNow,
                    ChangedBy = changedBy,
                    Reason = priceChangeReason
                });
        }

        product.SellingPrice = vm.SellingPrice;
        product.SalePrice = vm.SalePrice;
        product.SaleStartDate = vm.SaleStartDate;
        product.SaleEndDate = vm.SaleEndDate;
        product.Availability = vm.Availability;
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
    public async Task<IActionResult> DeleteImage(
        int productImageId,
        int productId)
    {
        var image = await _context.ProductImages
            .FirstOrDefaultAsync(i =>
                i.ProductImageId == productImageId &&
                i.ProductId == productId);

        if (image is null)
            return NotFound();

        var wasPrimary = image.IsPrimary;

        _fileUploadService.DeleteProductImage(image.ImageUrl);
        _context.ProductImages.Remove(image);

        if (wasPrimary)
        {
            var replacement = await _context.ProductImages
                .Where(i => i.ProductId == productId &&
                            i.ProductImageId != productImageId)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefaultAsync();

            if (replacement is not null)
                replacement.IsPrimary = true;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryImage(
        int productImageId,
        int productId)
    {
        var images = await _context.ProductImages
            .Where(i => i.ProductId == productId)
            .ToListAsync();

        var target = images.FirstOrDefault(i =>
            i.ProductImageId == productImageId);

        if (target is null)
            return NotFound();

        foreach (var image in images)
            image.IsPrimary = image.ProductImageId == productImageId;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplacePrimaryImage(
        int productId,
        IFormFile? replacement)
    {
        if (replacement is null || replacement.Length == 0)
        {
            TempData["Error"] = "Please select an image to replace the primary image.";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product is null)
            return NotFound();

        var oldPrimary = product.Images.FirstOrDefault(i => i.IsPrimary);
        var newUrl = await _fileUploadService.SaveProductImageAsync(replacement);

        if (oldPrimary is not null)
        {
            _fileUploadService.DeleteProductImage(oldPrimary.ImageUrl);
            _context.ProductImages.Remove(oldPrimary);
        }

        product.Images.Add(new ProductImage
        {
            ImageUrl = newUrl,
            IsPrimary = true,
            DisplayOrder = oldPrimary?.DisplayOrder ?? 0
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Primary image replaced.";
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var product = await _context.Products
            .FindAsync(id);

        if (product is null)
            return NotFound();

        product.IsActive = false;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Product deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Brand?> ResolveBrandAsync(ProductFormViewModel vm)
    {
        var name = vm.BrandName?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return null;

        var brand = await _context.Brands
            .FirstOrDefaultAsync(b =>
                EF.Functions.ILike(b.BrandName, name));

        var country = NullIfWhiteSpace(vm.BrandCountry);

        if (brand is null)
        {
            brand = new Brand
            {
                BrandName = name,
                Country = country,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Brands.Add(brand);
            return brand;
        }

        if (!string.IsNullOrWhiteSpace(country))
            brand.Country = country;

        brand.IsActive = true;
        return brand;
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task ValidateProductFormAsync(
        ProductFormViewModel vm)
    {
        if (vm.SalePrice.HasValue &&
            !vm.SellingPrice.HasValue)
        {
            ModelState.AddModelError(
                nameof(vm.SalePrice),
                "A regular selling price is required before setting a sale price.");
        }

        if (vm.SalePrice.HasValue &&
            vm.SellingPrice.HasValue &&
            vm.SalePrice.Value >= vm.SellingPrice.Value)
        {
            ModelState.AddModelError(
                nameof(vm.SalePrice),
                "Sale price must be lower than the regular selling price.");
        }

        if (vm.SaleStartDate.HasValue &&
            vm.SaleEndDate.HasValue &&
            vm.SaleEndDate.Value < vm.SaleStartDate.Value)
        {
            ModelState.AddModelError(
                nameof(vm.SaleEndDate),
                "Sale end date cannot be earlier than the sale start date.");
        }

        if (vm.SalePrice.HasValue &&
            !vm.SaleStartDate.HasValue &&
            vm.SaleEndDate.HasValue)
        {
            ModelState.AddModelError(
                nameof(vm.SaleStartDate),
                "Set a sale start date when using a sale end date.");
        }

        await Task.CompletedTask;
    }

    private async Task SaveNewImagesAsync(
        Product product,
        List<IFormFile>? files)
    {
        if (files is null || files.Count == 0)
            return;

        var hasPrimaryAlready =
            product.Images.Any(i => i.IsPrimary);

        var nextDisplayOrder =
            product.Images.Count == 0
                ? 0
                : product.Images.Max(i => i.DisplayOrder) + 1;

        foreach (var file in files.Where(f => f.Length > 0))
        {
            var url =
                await _fileUploadService
                    .SaveProductImageAsync(file);

            product.Images.Add(
                new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = !hasPrimaryAlready,
                    DisplayOrder = nextDisplayOrder++
                });

            hasPrimaryAlready = true;
        }
    }

    private async Task<List<SelectListItemModel>>
        GetCategoryOptionsAsync() =>
        await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryName)
            .Select(c =>
                new SelectListItemModel
                {
                    Value = c.CategoryId,
                    Text = c.CategoryName
                })
            .ToListAsync();

    private async Task<List<SelectListItemModel>>
        GetBrandOptionsAsync() =>
        await _context.Brands
            .Where(b => b.IsActive)
            .OrderBy(b => b.BrandName)
            .Select(b =>
                new SelectListItemModel
                {
                    Value = b.BrandId,
                    Text = b.BrandName
                })
            .ToListAsync();
}
