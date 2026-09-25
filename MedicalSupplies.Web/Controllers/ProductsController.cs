using MedicalSupplies.Infrastructure.Data;
using MedicalSupplies.Web.ViewModels.Admin;
using MedicalSupplies.Web.ViewModels.Catalogue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupplies.Web.Controllers;

/// <summary>Public-facing catalogue: search, category/brand filters, product details.</summary>
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? q, int? categoryId, int? brandId)
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
            query = query.Where(p => p.ProductName.Contains(q) || p.ProductCode.Contains(q));
        }
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }
        if (brandId.HasValue)
        {
            query = query.Where(p => p.BrandId == brandId.Value);
        }

        var products = await query.OrderBy(p => p.ProductName).ToListAsync();

        var vm = new CatalogueListViewModel
        {
            SearchTerm = q,
            CategoryId = categoryId,
            BrandId = brandId,
            TotalResults = products.Count,
            Products = products.Select(ToCard).ToList(),
            CategoryOptions = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItemModel { Value = c.CategoryId, Text = c.CategoryName })
                .ToListAsync(),
            BrandOptions = await _context.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.BrandName)
                .Select(b => new SelectListItemModel { Value = b.BrandId, Text = b.BrandName })
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
            .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

        if (product is null) return NotFound();

        var related = await _context.Products
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.ProductId != id)
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
            InStock = IsInStock(product.Batches, product.RequiresBatchTracking),
            ImageUrls = product.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).ToList(),
            RelatedProducts = related.Select(ToCard).ToList()
        };

        return View(vm);
    }

    private static CatalogueProductCardViewModel ToCard(Core.Entities.Product p) => new()
    {
        ProductId = p.ProductId,
        ProductCode = p.ProductCode,
        ProductName = p.ProductName,
        PrimaryImageUrl = p.Images.OrderByDescending(i => i.IsPrimary).FirstOrDefault()?.ImageUrl,
        CategoryName = p.Category.CategoryName,
        BrandName = p.Brand?.BrandName,
        PackSize = p.PackSize,
        InStock = IsInStock(p.Batches, p.RequiresBatchTracking)
    };

    private static bool IsInStock(ICollection<Core.Entities.InventoryBatch> batches, bool requiresBatchTracking)
    {
        // Equipment that isn't batch-tracked is treated as available on request.
        if (!requiresBatchTracking) return true;
        return batches.Any(b => b.Status == Core.Enums.BatchStatus.Active && b.QuantityAvailable > 0);
    }
}
