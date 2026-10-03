using MedicalSupplies.Core.Enums;
using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Admin;
using MedicalSupplies.Web.ViewModels.Catalogue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Controllers;

/// <summary>
/// Public-facing catalogue: search, category/brand filters, product details.
/// Catalogue availability is separate from physical inventory. For products
/// marked InStock that use batch tracking, actual usable inventory is checked
/// before displaying the customer-facing status.
/// </summary>
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context) =>
        _context = context;

    public async Task<IActionResult> Index(
        string? q,
        int? categoryId,
        int? brandId)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Batches)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var searchTerm = q.Trim();

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

        var products = await query
            .OrderBy(p => p.ProductName)
            .ToListAsync();

        var vm = new CatalogueListViewModel
        {
            SearchTerm = q,
            CategoryId = categoryId,
            BrandId = brandId,
            TotalResults = products.Count,
            Products = products
                .Select(ToCard)
                .ToList(),

            CategoryOptions = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItemModel
                {
                    Value = c.CategoryId,
                    Text = c.CategoryName
                })
                .ToListAsync(),

            BrandOptions = await _context.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.BrandName)
                .Select(b => new SelectListItemModel
                {
                    Value = b.BrandId,
                    Text = b.BrandName
                })
                .ToListAsync()
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Batches)
            .FirstOrDefaultAsync(
                p => p.ProductId == id &&
                     p.IsActive);

        if (product is null)
            return NotFound();

        var related = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Batches)
            .Where(
                p => p.IsActive &&
                     p.CategoryId == product.CategoryId &&
                     p.ProductId != id)
            .OrderBy(p => p.ProductName)
            .Take(4)
            .ToListAsync();

        var vm = new ProductDetailsViewModel
        {
            ProductId = product.ProductId,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            CategoryName = product.Category.CategoryName,
            BrandName = product.Brand?.BrandName,
            Description = product.Description,
            Specifications = product.Specifications,
            UnitOfMeasure = product.UnitOfMeasure,
            PackSize = product.PackSize,

            Availability = GetEffectiveAvailability(
                product.Availability,
                product.Batches,
                product.RequiresBatchTracking),

            InStock = IsInStock(
                product.Batches,
                product.RequiresBatchTracking),

            SellingPrice = product.SellingPrice,
            SalePrice = product.SalePrice,

            IsOnSale = IsSaleActive(
                product.SellingPrice,
                product.SalePrice,
                product.SaleStartDate,
                product.SaleEndDate),

            ImageUrls = product.Images
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.DisplayOrder)
                .Select(i => i.ImageUrl)
                .ToList(),

            RelatedProducts = related
                .Select(ToCard)
                .ToList()
        };

        return View(vm);
    }

    private static CatalogueProductCardViewModel ToCard(
        MedicalSupplies.Core.Entities.Product p)
    {
        return new CatalogueProductCardViewModel
        {
            ProductId = p.ProductId,
            ProductCode = p.ProductCode,
            ProductName = p.ProductName,

            PrimaryImageUrl = p.Images
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.DisplayOrder)
                .FirstOrDefault()
                ?.ImageUrl,

            CategoryName = p.Category.CategoryName,
            BrandName = p.Brand?.BrandName,
            PackSize = p.PackSize,

            Availability = GetEffectiveAvailability(
                p.Availability,
                p.Batches,
                p.RequiresBatchTracking),

            InStock = IsInStock(
                p.Batches,
                p.RequiresBatchTracking),

            SellingPrice = p.SellingPrice,
            SalePrice = p.SalePrice,

            IsOnSale = IsSaleActive(
                p.SellingPrice,
                p.SalePrice,
                p.SaleStartDate,
                p.SaleEndDate)
        };
    }

    private static ProductAvailability GetEffectiveAvailability(
        ProductAvailability availability,
        ICollection<MedicalSupplies.Core.Entities.InventoryBatch> batches,
        bool requiresBatchTracking)
    {
        if (availability != ProductAvailability.InStock)
            return availability;

        return IsInStock(
            batches,
            requiresBatchTracking)
            ? ProductAvailability.InStock
            : ProductAvailability.OutOfStock;
    }

    private static bool IsInStock(
        ICollection<MedicalSupplies.Core.Entities.InventoryBatch> batches,
        bool requiresBatchTracking)
    {
        // Non-batch products such as durable equipment can be explicitly
        // listed as InStock without requiring inventory batches.
        if (!requiresBatchTracking)
            return true;

        var today = DateOnly.FromDateTime(
            DateTime.UtcNow.AddHours(3));

        return batches.Any(
            b =>
                b.Status == BatchStatus.Active &&
                b.QuantityAvailable > 0 &&
                (
                    !b.ExpiryDate.HasValue ||
                    b.ExpiryDate.Value >= today
                ));
    }

    private static bool IsSaleActive(
        decimal? sellingPrice,
        decimal? salePrice,
        DateOnly? saleStartDate,
        DateOnly? saleEndDate)
    {
        if (!sellingPrice.HasValue ||
            !salePrice.HasValue ||
            salePrice.Value >= sellingPrice.Value)
        {
            return false;
        }

        var today = DateOnly.FromDateTime(
            DateTime.UtcNow.AddHours(3));

        if (saleStartDate.HasValue &&
            today < saleStartDate.Value)
        {
            return false;
        }

        if (saleEndDate.HasValue &&
            today > saleEndDate.Value)
        {
            return false;
        }

        return true;
    }
}
